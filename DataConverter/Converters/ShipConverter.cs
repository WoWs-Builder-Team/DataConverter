using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DataConverter.Data;
using DataConverter.JsonData;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;
using Hull = WoWsShipBuilder.DataStructures.Hull;
using ShipUpgrade = WoWsShipBuilder.DataStructures.ShipUpgrade;

namespace DataConverter.Converters;

public static class ShipConverter
{
    private static readonly ConcurrentBag<string> ReportedTypes = new();

    public static List<ShipSummary> ShipSummaries { get; } = new();

    public static Dictionary<string, Ship> ConvertShips(IEnumerable<WgShip> wgShipList, string nation, ShiptoolData shiptoolData, ILogger? logger)
    {
        var results = new Dictionary<string, Ship>();
        var count = 0;

        Dictionary<string, string> shipToPreviousShipMapper = new Dictionary<string, string>();
        Dictionary<string, List<string>> shipToNextShipMapper = new Dictionary<string, List<string>>();

        foreach (WgShip wgShip in wgShipList)
        {
            if (wgShip.TypeInfo.Species.Equals(ShipClass.Auxiliary.ToString()) || wgShip.Group.Equals("clan") || wgShip.Group.Equals("disabled") ||
                wgShip.Group.Equals("preserved") || wgShip.Group.Equals("unavailable") || wgShip.Group.Equals("coopOnly"))
            {
                continue;
            }

            DataCache.TranslationNames.Add(wgShip.Index);
            var stShip = shiptoolData.Ship.FirstOrDefault(s => s.Index.Equals(wgShip.Index));
            var ship = new Ship
            {
                Id = wgShip.Id,
                Index = wgShip.Index,
                Name = wgShip.Name,
                Tier = wgShip.Level,
                ShipClass = ProcessShipClass(wgShip.TypeInfo.Species),
                ShipCategory = ProcessShipCategory(wgShip.Group, wgShip.Level),
                ShipNation = Enum.Parse<Nation>(wgShip.TypeInfo.Nation.Replace("_", string.Empty), true),
                MainBatteryModuleList = ProcessMainBattery(wgShip, stShip),
                ShipUpgradeInfo = ProcessUpgradeInfo(wgShip, logger),
                FireControlList = ProcessFireControl(wgShip),
                TorpedoModules = ProcessTorpedoes(wgShip, stShip),
                Engines = ProcessEngines(wgShip),
                ShipConsumable = ProcessConsumables(wgShip),
                AirStrikes = ProcessAirstrikes(wgShip),
                PingerGunList = ProcessPingerGuns(wgShip),
                SpecialAbility = ProcessSpecialAbility(wgShip),
                Permoflages = wgShip.Permoflages,
            };

            DataCache.TranslationNames.UnionWith(wgShip.Permoflages);

            ship.Hulls = ProcessShipHull(wgShip, ship.ShipUpgradeInfo);
            ship.CvPlanes = ProcessPlanes(wgShip, ship.ShipUpgradeInfo);
            results[ship.Index] = ship;

            if (ship.ShipCategory == ShipCategory.TechTree)
            {
                shipToNextShipMapper[ship.Index] = ship.ShipUpgradeInfo.ShipUpgrades
                    .SelectMany(shipUpgrade => shipUpgrade.NextShips)
                    .Select(shipName => shipName.Split('_').First())
                    .ToList();
            }

            count++;
            if (count % 10 == 0)
            {
                logger?.LogInformation("Processed {count} ships for {nation}", count, nation);
            }
        }

        foreach ((string shipIndex, List<string> nextShipIndexes) in shipToNextShipMapper)
        {
            foreach (string nextShipIndex in nextShipIndexes)
            {
                shipToPreviousShipMapper[nextShipIndex] = shipIndex;
            }
        }

        foreach ((_, Ship ship) in results)
        {
            shipToPreviousShipMapper.TryGetValue(ship.Index, out string? previousShip);
            shipToNextShipMapper.TryGetValue(ship.Index, out List<string>? nextShips);
            ShipSummaries.Add(new(ship.Index, ship.ShipNation, ship.Tier, ship.ShipClass, ship.ShipCategory, previousShip, nextShips));
        }

        return results;
    }

    #region Component converters

    private static BurstModeAbility? ProcessBurstModeAbility(WgBurstArtilleryModule? module)
    {
        if (module != null)
        {
            var burstAbility = new BurstModeAbility()
            {
                ShotInBurst = module.ShotsCount,
                ReloadAfterBurst = module.FullReloadTime,
                ReloadDuringBurst = module.BurstReloadTime,
                Modifiers = module.Modifiers,
            };
            DataCache.TranslationNames.UnionWith(burstAbility.Modifiers.Keys);
            return burstAbility;
        }

        return null;
    }

    private static SpecialAbility? ProcessSpecialAbility(WgShip wgShip)
    {
        Dictionary<string, WgSpecialAbility> wgSpecialAbilityList = wgShip.ModulesArmaments.ModulesOfType<WgSpecialAbility>();
        if (wgSpecialAbilityList.Count > 1)
        {
            throw new InvalidOperationException($"Too many special abilities for ship {wgShip.Index}");
        }
        else if (wgSpecialAbilityList.Count == 1)
        {
            var wgAbility = wgSpecialAbilityList.Values.First().RageMode;
            var specialAbility = new SpecialAbility()
            {
                Duration = wgAbility.BoostDuration,
                Modifiers = wgAbility.Modifiers,
                Name = wgAbility.RageModeName,
                RadiusForSuccessfulHits = wgAbility.Radius,
                RequiredHits = wgAbility.RequiredHits,
            };
            DataCache.TranslationNames.Add(specialAbility.Name);
            DataCache.TranslationNames.Add("RageMode");
            DataCache.TranslationNames.UnionWith(specialAbility.Modifiers.Keys);
            return specialAbility;
        }

        return null;
    }

    private static ShipCategory ProcessShipCategory(string wgCategory, int tier)
    {
        return wgCategory switch
        {
            "start" => ShipCategory.TechTree,
            "upgradeable" => ShipCategory.TechTree,
            "premium" => ShipCategory.Premium,
            "ultimate" => ShipCategory.Special,
            "specialUnsellable" when tier < 10 => ShipCategory.Premium,
            "specialUnsellable" when tier == 10 => ShipCategory.Special,
            "demoWithoutStats" => ShipCategory.TestShip,
            "demoWithStats" => ShipCategory.TestShip,
            "special" when tier < 10 => ShipCategory.Premium,
            "special" when tier == 10 => ShipCategory.Special,
            "disabled" => ShipCategory.Disabled,
            "preserved" => ShipCategory.Disabled,
            "clan" => ShipCategory.Clan,
            "earlyAccess" => ShipCategory.TechTree,
            "upgradeableExclusive" => ShipCategory.Premium,
            "upgradeableUltimate" => ShipCategory.Special,
            "unavailable" => ShipCategory.Disabled,
            "legendaryBattle" => ShipCategory.TechTree,
            "superShip" => ShipCategory.SuperShip,
            "coopOnly" => ShipCategory.Disabled, _ => throw new InvalidOperationException("Ship category not recognized: " + wgCategory),
        };
    }

    private static ShipClass ProcessShipClass(string shipClass)
    {
        return shipClass.ToLowerInvariant() switch
        {
            "cruiser" => ShipClass.Cruiser,
            "destroyer" => ShipClass.Destroyer,
            "battleship" => ShipClass.Battleship,
            "aircarrier" => ShipClass.AirCarrier,
            "submarine" => ShipClass.Submarine,
            "auxiliary" => ShipClass.Auxiliary,
            _ => throw new InvalidOperationException("Ship class not recognized."),
        };
    }

    private static UpgradeInfo ProcessUpgradeInfo(WgShip wgShip, ILogger? logger)
    {
        var upgradeInfo = new UpgradeInfo
        {
            ShipUpgrades = new List<ShipUpgrade>(),
            CostCredits = wgShip.ShipUpgradeInfo.CostCr,
            CostGold = wgShip.ShipUpgradeInfo.CostGold,
            CostXp = wgShip.ShipUpgradeInfo.CostXp,
            CostSaleGold = wgShip.ShipUpgradeInfo.CostSaleGold,
            Value = wgShip.ShipUpgradeInfo.Value,
        };

        foreach ((string wgName, var upgrade) in wgShip.ShipUpgradeInfo.ConvertedUpgrades)
        {
            var newUpgrade = new ShipUpgrade
            {
                Name = wgName,
                Components = upgrade.Components.Select(entry => (FindModuleType(entry.Key, logger), entry.Value))
                    .Where(entry => entry.Item1 != ComponentType.None)
                    .ToDictionary(entry => entry.Item1, entry => entry.Value),
                UcType = FindModuleType(upgrade.UcType, logger),
                CanBuy = upgrade.CanBuy,
                NextShips = upgrade.NextShips,
                Prev = upgrade.Prev,
            };
            if (newUpgrade.UcType != ComponentType.FlightControl)
            {
                upgradeInfo.ShipUpgrades.Add(newUpgrade);
            }
        }

        return upgradeInfo;
    }

    private static Dictionary<string, TurretModule> ProcessMainBattery(WgShip wgShip, ShiptoolShip? stShip)
    {
        var resultDictionary = new Dictionary<string, TurretModule>();
        Dictionary<string, WgMainBattery> artilleryModules = wgShip.ModulesArmaments.ModulesOfType<WgMainBattery>();

        foreach ((string key, WgMainBattery wgMainBattery) in artilleryModules)
        {
            var stMainBatteryModule = stShip?.GetArmamentModule(key);
            var turretModule = new TurretModule
            {
                Sigma = wgMainBattery.SigmaCount,
                MaxRange = wgMainBattery.MaxDist,
                Guns = wgMainBattery.Guns.Select(entry => ConvertMainBatteryGun(entry.Value, entry.Key, stMainBatteryModule)).ToList(),
                BurstModeAbility = ProcessBurstModeAbility(wgMainBattery.BurstArtilleryModule),
            };
            var dispersionGun = wgMainBattery.Guns.Values.First();
            var turretDispersion = new Dispersion
            {
                IdealRadius = dispersionGun.IdealRadius,
                MinRadius = dispersionGun.MinRadius,
                IdealDistance = dispersionGun.IdealDistance,
                TaperDist = wgMainBattery.TaperDist,
                RadiusOnZero = dispersionGun.RadiusOnZero,
                RadiusOnDelim = dispersionGun.RadiusOnDelim,
                RadiusOnMax = dispersionGun.RadiusOnMax,
                Delim = dispersionGun.Delim,
            };

            var maxRange = decimal.ToDouble(turretModule.MaxRange);
            turretDispersion.MaximumHorizontalDispersion = Math.Round(Convert.ToDecimal(turretDispersion.CalculateHorizontalDispersion(maxRange)), 1);
            turretDispersion.MaximumVerticalDispersion = Math.Round(Convert.ToDecimal(turretDispersion.CalculateVerticalDispersion(maxRange)), 1);

            turretModule.DispersionValues = turretDispersion;
            DataCache.TranslationNames.UnionWith(turretModule.Guns.Select(gun => gun.Name).Distinct());

            var targetAntiAir = new AntiAir();
            AssignAurasToProperty(wgMainBattery.AntiAirAuras, targetAntiAir);
            if (targetAntiAir.LongRangeAura?.ConstantDps > 0)
            {
                turretModule.AntiAir = targetAntiAir.LongRangeAura;
            }

            resultDictionary[key] = turretModule;
        }

        return resultDictionary;
    }

    private static Gun ConvertMainBatteryGun(WgGun wgGun, string mainGunKey, ShiptoolArmamentModule? stGuns)
    {
        var newGun = wgGun.ConvertData();
        newGun.WgGunIndex = mainGunKey;
        var stGun = stGuns?.GetGunData(mainGunKey);
        newGun.BaseAngle = stGun?.BaseAngle ?? (newGun.VerticalPosition < 3 ? 0 : 180);

        return newGun;
    }

    private static Dictionary<string, Hull> ProcessShipHull(WgShip wgShip, UpgradeInfo upgradeInfo)
    {
        var resultDictionary = new Dictionary<string, Hull>();
        Dictionary<string, WgHull> hullModules = wgShip.ModulesArmaments.ModulesOfType<WgHull>();

        foreach ((string key, WgHull wgHull) in hullModules)
        {
            ShipUpgrade hullUpgradeInfo = upgradeInfo.ShipUpgrades.First(upgradeEntry =>
                upgradeEntry.Components.ContainsKey(ComponentType.Hull) && upgradeEntry.Components[ComponentType.Hull].Contains(key));

            // Initialize basic hull data.
            var hullModule = new Hull
            {
                Health = wgHull.Health,
                MaxSpeed = wgHull.MaxSpeed,
                RudderTime = wgHull.RudderTime,
                SpeedCoef = wgHull.SpeedCoef,
                SteeringGearArmorCoeff = wgHull.Sg.ArmorCoeff,
                SmokeFiringDetection = wgHull.VisibilityCoefGkInSmoke,
                SurfaceDetection = wgHull.VisibilityFactor,
                AirDetection = wgHull.VisibilityFactorByPlane,
                DetectionBySubPeriscope = wgHull.VisibilityFactorsBySubmarine["PERISCOPE"],
                DetectionBySubOperating = wgHull.VisibilityFactorsBySubmarine["DEEP_WATER_INVUL"],
                FireSpots = wgHull.BurnNodes.Length,
                FireResistance = wgHull.BurnNodes[0][0],
                FireTickDamage = wgHull.BurnNodes[0][1],
                FireDuration = wgHull.BurnNodes[0][2],
                FloodingSpots = wgHull.FloodNodes.Length,
                FloodingResistance = wgHull.FloodNodes[0][0],
                FloodingTickDamage = wgHull.FloodNodes[0][1],
                FloodingDuration = wgHull.FloodNodes[0][2],
                TurningRadius = wgHull.TurningRadius,
                Tonnage = wgHull.Tonnage,
                EnginePower = wgHull.EnginePower,
            };

            //Process hit locations
            List<HitLocation> hitLocations = new();
            if (!wgHull.Cas.HlType.Equals(string.Empty))
            {
                HitLocation hl = new()
                {
                    Name = ShipHitLocation.Casemate,
                    Type = wgHull.Cas.HlType,
                    RepairableDamage = wgHull.Cas.RegeneratedHpPart,
                    Hp = wgHull.Cas.MaxHp,
                };
                hitLocations.Add(hl);
            }

            if (!wgHull.Bow.HlType.Equals(string.Empty))
            {
                HitLocation hl = new()
                {
                    Name = ShipHitLocation.Bow,
                    Type = wgHull.Bow.HlType,
                    RepairableDamage = wgHull.Bow.RegeneratedHpPart,
                    Hp = wgHull.Bow.MaxHp,
                };
                hitLocations.Add(hl);
            }

            if (!wgHull.Ss.HlType.Equals(string.Empty))
            {
                HitLocation hl = new()
                {
                    Name = ShipHitLocation.Superstructure,
                    Type = wgHull.Ss.HlType,
                    RepairableDamage = wgHull.Ss.RegeneratedHpPart,
                    Hp = wgHull.Ss.MaxHp,
                };
                hitLocations.Add(hl);
            }

            if (!wgHull.St.HlType.Equals(string.Empty))
            {
                HitLocation hl = new()
                {
                    Name = ShipHitLocation.Stern,
                    Type = wgHull.St.HlType,
                    RepairableDamage = wgHull.St.RegeneratedHpPart,
                    Hp = wgHull.St.MaxHp,
                };
                hitLocations.Add(hl);
            }

            if (!wgHull.Ssc.HlType.Equals(string.Empty))
            {
                HitLocation hl = new()
                {
                    Name = ShipHitLocation.AuxiliaryRooms,
                    Type = wgHull.Ssc.HlType,
                    RepairableDamage = wgHull.Ssc.RegeneratedHpPart,
                    Hp = wgHull.Ssc.MaxHp,
                };
                hitLocations.Add(hl);
            }

            if (!wgHull.Hull.HlType.Equals(string.Empty))
            {
                HitLocation hl = new()
                {
                    Name = ShipHitLocation.Hull,
                    Type = wgHull.Hull.HlType,
                    RepairableDamage = wgHull.Hull.RegeneratedHpPart,
                    Hp = wgHull.Hull.MaxHp,
                };
                hitLocations.Add(hl);
            }

            if (!wgHull.Cit.HlType.Equals(string.Empty))
            {
                HitLocation hl = new()
                {
                    Name = ShipHitLocation.Citadel,
                    Type = wgHull.Cit.HlType,
                    RepairableDamage = wgHull.Cit.RegeneratedHpPart,
                    Hp = wgHull.Cit.MaxHp,
                };
                hitLocations.Add(hl);
            }

            hullModule.HitLocations = hitLocations;

            //process MaxSpeedAtBuoyancyState
            Dictionary<SubsBuoyancyStates, decimal> maxSpeedAtBuoyancyStateCoeff = new();
            if (ProcessShipClass(wgShip.TypeInfo.Species) == ShipClass.Submarine)
            {
                foreach ((string state, object[] coeff) in wgHull.BuoyancyStates)
                {
                    var depth = state switch
                    {
                        "DEEP_WATER_INVUL" => SubsBuoyancyStates.DeepWater,
                        "PERISCOPE" => SubsBuoyancyStates.Periscope,
                        "SURFACE" => SubsBuoyancyStates.Surface,
                        _ => throw new InvalidOperationException("Buoyancy state not recognized: " + wgHull),
                    };
                    maxSpeedAtBuoyancyStateCoeff.Add(depth, (decimal)(double)coeff[1]);
                }
            }

            hullModule.MaxSpeedAtBuoyancyStateCoeff = maxSpeedAtBuoyancyStateCoeff;

            //Process ship size
            ShipSize dim = new()
            {
                Length = wgHull.Size[0],
                Width = wgHull.Size[1],
                Height = wgHull.Size[2],
            };
            hullModule.Sizes = dim;

            // Process anti-air data
            var antiAir = new AntiAir();

            string[] components = hullUpgradeInfo.Components[ComponentType.Secondary];
            if (components.Length > 0)
            {
                var wgHullSecondary = (WgAtba)wgShip.ModulesArmaments[components.First()];
                AssignAurasToProperty(wgHullSecondary.AntiAirAuras, antiAir);

                // Process secondaries
                var secondary = new TurretModule
                {
                    Sigma = wgHullSecondary.SigmaCount,
                    MaxRange = wgHullSecondary.MaxDist,
                    Guns = wgHullSecondary.AntiAirAndSecondaries.Values.Select(secondaryGun => secondaryGun.ConvertData()).ToList(),
                };
                DataCache.TranslationNames.UnionWith(secondary.Guns.Select(gun => gun.Name).Distinct());
                hullModule.SecondaryModule = secondary;
            }

            if (hullUpgradeInfo.Components.TryGetValue(ComponentType.AirDefense, out string[]? airDefenseKeys))
            {
                foreach (string airDefenseKey in airDefenseKeys)
                {
                    var airDefenseArmament = (WgAirDefense)wgShip.ModulesArmaments[airDefenseKey];
                    AssignAurasToProperty(airDefenseArmament.AntiAirAuras, antiAir);
                }
            }

            hullModule.AntiAir = antiAir;

            // Process depth charge data
            if (hullUpgradeInfo.Components.TryGetValue(ComponentType.DepthCharges, out string[]? depthChargeKey) && depthChargeKey.Length > 0)
            {
                var wgDepthChargeArray = (WgDepthChargesArray)wgShip.ModulesArmaments[depthChargeKey.First()];
                hullModule.DepthChargeArray = new DepthChargeArray
                {
                    MaxPacks = wgDepthChargeArray.MaxPacks,
                    NumShots = wgDepthChargeArray.NumShots,
                    Reload = wgDepthChargeArray.ReloadTime,
                    DepthCharges = wgDepthChargeArray.DepthCharges.Select(entry => entry.Value.ConvertData()).ToList(),
                };
                DataCache.TranslationNames.UnionWith(hullModule.DepthChargeArray.DepthCharges.Select(depthChargeLauncher => depthChargeLauncher.Name)
                    .Distinct());
            }

            resultDictionary[key] = hullModule;
        }

        return resultDictionary;
    }

    private static Dictionary<string, FireControl> ProcessFireControl(WgShip wgShip)
    {
        var resultDictionary = new Dictionary<string, FireControl>();

        Dictionary<string, WgFireControl> fireControlModules = wgShip.ModulesArmaments.ModulesOfType<WgFireControl>();

        foreach ((string key, WgFireControl wgFireControl) in fireControlModules)
        {
            var fireControl = new FireControl
            {
                MaxRangeModifier = wgFireControl.MaxDistCoef,
                SigmaModifier = wgFireControl.SigmaCountCoef,
            };
            resultDictionary[key] = fireControl;
        }

        return resultDictionary;
    }

    private static Dictionary<string, TorpedoModule> ProcessTorpedoes(WgShip wgShip, ShiptoolShip? stShip)
    {
        var resultDictionary = new Dictionary<string, TorpedoModule>();
        Dictionary<string, WgTorpedoArray> wgTorpedoList = wgShip.ModulesArmaments.ModulesOfType<WgTorpedoArray>();

        foreach ((string key, WgTorpedoArray wgTorpedoArray) in wgTorpedoList)
        {
            var stTorpedoes = stShip?.GetArmamentModule(key);
            var torpedoModule = new TorpedoModule
            {
                TimeToChangeAmmo = wgTorpedoArray.TorpedoArray.Values.Any(launcher => launcher.AmmoList.Length > 1) ? wgTorpedoArray.TimeToChangeAmmo : 0,
                TorpedoLaunchers = wgTorpedoArray.TorpedoArray.Select(entry => ConvertWgTorpedoLauncher(entry.Key, entry.Value, stTorpedoes)).ToList(),
            };
            DataCache.TranslationNames.UnionWith(torpedoModule.TorpedoLaunchers.Select(launcher => launcher.Name).Distinct());

            resultDictionary[key] = torpedoModule;
        }

        return resultDictionary;
    }

    private static TorpedoLauncher ConvertWgTorpedoLauncher(string wgKey, WgTorpedoLauncher wgTorpedoLauncher, ShiptoolArmamentModule? stModule)
    {
        var launcher = wgTorpedoLauncher.ConvertData();
        launcher.BaseAngle = stModule?.GetGunData(wgKey)?.BaseAngle ?? (launcher.VerticalPosition < 3 ? 0 : 180);
        return launcher;
    }

    private static Dictionary<string, Engine> ProcessEngines(WgShip wgShip)
    {
        var resultDictionary = new Dictionary<string, Engine>();
        Dictionary<string, WgEngine> wgEngineList = wgShip.ModulesArmaments.ModulesOfType<WgEngine>();

        foreach ((string key, WgEngine wgEngine) in wgEngineList)
        {
            var engine = new Engine
            {
                BackwardEngineUpTime = wgEngine.BackwardEngineUpTime,
                ForwardEngineUpTime = wgEngine.ForwardEngineUpTime,
                BackwardEngineForsag = wgEngine.BackwardEngineForsag,
                BackwardEngineForsagMaxSpeed = wgEngine.BackwardEngineForsagMaxSpeed,
                ForwardEngineForsag = wgEngine.ForwardEngineForsag,
                ForwardEngineForsagMaxSpeed = wgEngine.ForwardEngineForsagMaxSpeed,
                SpeedCoef = wgEngine.SpeedCoef,
                ArmorCoeff = wgEngine.HitLocationEngine.ArmorCoeff,
            };
            resultDictionary[key] = engine;
        }

        return resultDictionary;
    }

    private static Dictionary<string, List<PlaneData>> ProcessPlanes(WgShip wgShip, UpgradeInfo upgradeInfo)
    {
        var resultDictionary = new Dictionary<string, List<PlaneData>>();
        foreach (PlaneType type in Enum.GetValues(typeof(PlaneType)))
        {
            IEnumerable<KeyValuePair<string, PlaneData>> planesOfType = upgradeInfo.ShipUpgrades
                .Where(upgrade => upgrade.UcType == type.ToComponentType())
                .Select(planeUpgrade => planeUpgrade.Components[type.ToComponentType()].First())
                .Select(moduleKey => (moduleKey, (WgPlane)wgShip.ModulesArmaments[moduleKey]))
                .SelectMany(wgPlane =>
                {
                    var plane = wgPlane.Item2;
                    if (plane.Planes != null)
                    {
                        // Process ships starting with patch 0.11.1
                        return plane.Planes.Select(planeName => new PlaneData { PlaneName = planeName, PlaneType = type })
                            .Select(data => new KeyValuePair<string, PlaneData>(wgPlane.moduleKey, data));
                    }

                    // Processing for older versions
                    return new List<KeyValuePair<string, PlaneData>>
                    {
                        new(wgPlane.moduleKey, new() { PlaneName = wgPlane.Item2.PlaneType, PlaneType = type, }),
                    };
                })
                .ToList();

            DataCache.TranslationNames.UnionWith(planesOfType.Select(planeEntry => planeEntry.Value.PlaneName).Distinct());
            foreach ((string moduleName, PlaneData planeData) in planesOfType)
            {
                if (resultDictionary.ContainsKey(moduleName))
                {
                    resultDictionary[moduleName].Add(planeData);
                }
                else
                {
                    resultDictionary[moduleName] = new() { planeData };
                }
            }
        }

        return resultDictionary;
    }

    private static List<ShipConsumable> ProcessConsumables(WgShip ship)
    {
        var resultList = new List<ShipConsumable>();
        foreach ((_, WgShipAbility wgAbility) in ship.ShipAbilities)
        {
            IEnumerable<ShipConsumable> consumableList = wgAbility.Abils
                .Select(ability => (AbilityName: ability[0], AbilityVariant: ability[1]))
                .Select(ability =>
                    new ShipConsumable
                    {
                        ConsumableName = ability.AbilityName,
                        ConsumableVariantName = ability.AbilityVariant,
                        Slot = wgAbility.Slot,
                    });
            resultList.AddRange(consumableList);
        }

        return resultList;
    }

    private static Dictionary<string, AirStrike> ProcessAirstrikes(WgShip wgShip)
    {
        Dictionary<string, AirStrike> result = wgShip.ModulesArmaments
            .ModulesOfType<WgAirSupport>()
            .ToDictionary(entry => entry.Key, entry => entry.Value.ConvertData());
        DataCache.TranslationNames.UnionWith(result.Values.Select(airStrike => airStrike.PlaneName).Distinct());
        return result;
    }

    private static Dictionary<string, PingerGun> ProcessPingerGuns(WgShip wgShip)
    {
        return wgShip.ModulesArmaments
            .ModulesOfType<WgPingerGun>()
            .ToDictionary(entry => entry.Key, entry => entry.Value.ConvertData());
    }

    #endregion

    #region Helper methods

    private static Dictionary<string, T> ModulesOfType<T>(this Dictionary<string, WgArmamentModule> thisDict) where T : WgArmamentModule
    {
        return thisDict.Where(module => module.Value is T)
            .ToDictionary(entry => entry.Key, entry => (T)entry.Value);
    }

    private static void AssignAurasToProperty(Dictionary<string, WgAaAura>? auras, AntiAir? targetAntiAir)
    {
        if (auras == null || targetAntiAir == null)
        {
            return;
        }

        foreach ((_, WgAaAura aura) in auras)
        {
            switch (aura.Type)
            {
                case "far":
                    if (targetAntiAir.LongRangeAura != null)
                    {
                        targetAntiAir.LongRangeAura += aura.ConvertData();
                    }
                    else
                    {
                        targetAntiAir.LongRangeAura = aura.ConvertData();
                    }

                    break;

                case "medium":
                    if (targetAntiAir.MediumRangeAura != null)
                    {
                        targetAntiAir.MediumRangeAura += aura.ConvertData();
                    }
                    else
                    {
                        targetAntiAir.MediumRangeAura = aura.ConvertData();
                    }

                    break;

                case "near":
                    if (targetAntiAir.ShortRangeAura != null)
                    {
                        targetAntiAir.ShortRangeAura += aura.ConvertData();
                    }
                    else
                    {
                        targetAntiAir.ShortRangeAura = aura.ConvertData();
                    }

                    break;
            }
        }
    }

    private static ComponentType FindModuleType(string rawModuleType, ILogger? logger)
    {
        rawModuleType = rawModuleType.TrimStart('_');
        string normalizedInput = rawModuleType.First().ToString().ToUpper() + rawModuleType[1..];
        ComponentType componentType = normalizedInput switch
        {
            "Artillery" => ComponentType.Artillery,
            "Engine" => ComponentType.Engine,
            "Hull" => ComponentType.Hull,
            "Suo" => ComponentType.Suo,
            "FireControl" => ComponentType.Suo,
            "Torpedoes" => ComponentType.Torpedoes,
            "FlightControl" => ComponentType.FlightControl,
            "Fighter" => ComponentType.Fighter,
            "DiveBomber" => ComponentType.DiveBomber,
            "TorpedoBomber" => ComponentType.TorpedoBomber,
            "SkipBomber" => ComponentType.SkipBomber,
            "Atba" => ComponentType.Secondary,
            "AirDefense" => ComponentType.AirDefense,
            "AirArmament" => ComponentType.AirArmament,
            "DepthCharges" => ComponentType.DepthCharges,
            "Sonar" => ComponentType.Sonar,
            _ => ComponentType.None,
        };

        if (componentType == ComponentType.None && !ReportedTypes.Contains(normalizedInput))
        {
            ReportedTypes.Add(normalizedInput);
            logger?.LogWarning("Cannot find type for provided string: {normalizedInput}", normalizedInput);
        }

        return componentType;
    }

    private static ComponentType ToComponentType(this PlaneType thisType)
    {
        return thisType switch
        {
            PlaneType.Fighter => ComponentType.Fighter,
            PlaneType.DiveBomber => ComponentType.DiveBomber,
            PlaneType.TorpedoBomber => ComponentType.TorpedoBomber,
            PlaneType.SkipBomber => ComponentType.SkipBomber,
            PlaneType.TacticalFighter => ComponentType.TacticalFighter,
            PlaneType.TacticalDiveBomber => ComponentType.TacticalDiveBomber,
            PlaneType.TacticalTorpedoBomber => ComponentType.TacticalTorpedoBomber,
            PlaneType.TacticalSkipBomber => ComponentType.TacticalSkipBomber,
            _ => throw new ArgumentOutOfRangeException(nameof(thisType), thisType, "Cannot process supplied plane type."),
        };
    }

    #endregion
}
