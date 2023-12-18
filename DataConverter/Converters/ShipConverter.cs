using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using DataConverter.Data;
using DataConverter.JsonData;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;
using Hull = WoWsShipBuilder.DataStructures.Ship.Hull;
using ShipUpgrade = WoWsShipBuilder.DataStructures.Ship.ShipUpgrade;

namespace DataConverter.Converters;

public static class ShipConverter
{
    private static readonly ConcurrentBag<string> ReportedTypes = new();

    public static List<ShipSummary> ShipSummaries { get; } = new();

    public static Dictionary<string, Ship> ConvertShips(IEnumerable<WgShip> wgShipList, string nation, ShiptoolData shiptoolData, ILogger? logger, Dictionary<string, Modifier> modifiersDictionary, Dictionary<long, int> techTreeShipsPositionsDictionary)
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
            var stShip = shiptoolData.Ship.Find(s => s.Index.Equals(wgShip.Index));
            var upgradeInfo = ProcessUpgradeInfo(wgShip, logger);
            var mainBatteries = ProcessMainBattery(wgShip, upgradeInfo, stShip, modifiersDictionary);
            var ship = new Ship
            {
                Id = wgShip.Id,
                Index = wgShip.Index,
                Name = wgShip.Name,
                Tier = wgShip.Level,
                ShipClass = ProcessShipClass(wgShip.TypeInfo.Species),
                ShipCategory = ProcessShipCategory(wgShip.Group, wgShip.Level),
                ShipNation = Enum.Parse<Nation>(wgShip.TypeInfo.Nation.Replace("_", string.Empty), true),
                MainBatteryModuleList = mainBatteries,
                ShipUpgradeInfo = upgradeInfo,
                FireControlList = ProcessFireControl(wgShip),
                TorpedoModules = ProcessTorpedoes(wgShip, upgradeInfo, stShip),
                Engines = ProcessEngines(wgShip),
                ShipConsumable = ProcessConsumables(wgShip),
                AirStrikes = ProcessAirstrikes(wgShip),
                PingerGunList = ProcessPingerGuns(wgShip),
                SpecialAbility = ProcessSpecialAbility(wgShip, logger, modifiersDictionary),
                Hulls = ProcessShipHull(wgShip, upgradeInfo),
                CvPlanes = ProcessPlanes(wgShip, upgradeInfo),
                ShellCompatibilities = CheckShellCompatibilities(mainBatteries, upgradeInfo),
                TechTreePosition = techTreeShipsPositionsDictionary.GetValueOrDefault(wgShip.Id),
            };

            DataCache.TranslationNames.UnionWith(wgShip.Permoflages);

            results[ship.Index] = ship;

            if (ship.ShipCategory == ShipCategory.TechTree)
            {
                shipToNextShipMapper[ship.Index] = ship.ShipUpgradeInfo.ShipUpgrades
                    .SelectMany(shipUpgrade => shipUpgrade.NextShips)
                    .Select(shipName => shipName.Split('_')[0])
                    .ToList();
            }

            count++;
            if (count % 10 == 0)
            {
                logger?.LogInformation("Processed {Count} ships for {Nation}", count, nation);
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
            ShipSummaries.Add(new(ship.Index, ship.ShipNation, ship.Tier, ship.ShipClass, ship.ShipCategory, previousShip, nextShips?.ToImmutableArray()));
        }

        return results;
    }

    #region Component converters

    private static BurstModeAbility? ProcessBurstModeAbility(WgBurstArtilleryModule? module, Dictionary<string, Modifier> modifiersDictionary, string shipName)
    {
        if (module != null)
        {
            var modifiers = new List<Modifier>();
            foreach (var (modifierName, modifierValue) in module.Modifiers)
            {
                modifiersDictionary.TryGetValue(modifierName, out Modifier? modifierData);
                modifiers.Add(new(modifierName, modifierValue, shipName,  modifierData));
            }

            var burstAbility = new BurstModeAbility
            {
                ShotInBurst = module.ShotsCount,
                ReloadAfterBurst = module.FullReloadTime,
                ReloadDuringBurst = module.BurstReloadTime,
                Modifiers = modifiers.ToImmutableList(),
            };

            DataCache.TranslationNames.UnionWith(burstAbility.Modifiers.Select(m => m.Name));
            return burstAbility;
        }

        return null;
    }

    private static SpecialAbility? ProcessSpecialAbility(WgShip wgShip, ILogger? logger, Dictionary<string, Modifier> modifiersDictionary)
    {
        Dictionary<string, WgSpecialAbility> wgSpecialAbilityList = wgShip.ModulesArmaments.ModulesOfType<WgSpecialAbility>();
        if (wgSpecialAbilityList.Count > 1)
        {
            logger?.LogWarning("Multiple special abilities for ship {Index}", wgShip.Index);
            var wgAbility = wgSpecialAbilityList.Values.First().RageMode;
            return ProcessRageMode(wgAbility, modifiersDictionary, wgShip.Name);
        }

        if (wgSpecialAbilityList.Count == 1)
        {
            var wgAbility = wgSpecialAbilityList.Values.First().RageMode;
            return ProcessRageMode(wgAbility, modifiersDictionary, wgShip.Name);
        }

        return null;
    }

    private static SpecialAbility ProcessRageMode(WgRageMode rageMode, Dictionary<string, Modifier> modifiersDictionary, string shipName)
    {
        var modifierList = rageMode.Modifiers.Where(x => x.Value.Type is JTokenType.Float or JTokenType.Integer)
            .Select(x =>
            {
                modifiersDictionary.TryGetValue(x.Key, out Modifier? modifierData);
                return new Modifier(x.Key, x.Value.Value<float>(), shipName, modifierData);
            }).ToList();
        foreach (var modifierObject in rageMode.Modifiers.Where(x => x.Value.Type is JTokenType.Object))
        {
            var key = modifierObject.Key;
            var modifiers = modifierObject.Value.ToObject<Dictionary<string, float>>();
            bool allEquals = modifiers!.Values.Distinct().Count() == 1;
            if (allEquals)
            {
                modifiersDictionary.TryGetValue(key, out Modifier? modifierData);
                modifierList.Add(new Modifier(key, modifiers.First().Value, shipName, modifierData));
            }
            else
            {
                foreach (var (modifierName, modifierValue) in modifiers)
                {
                    modifiersDictionary.TryGetValue($"{key}_{modifierName}", out Modifier? modifierData);
                    modifierList.Add(new Modifier($"{key}_{modifierName}", modifierValue, shipName, modifierData));
                }
            }
        }

        var specialAbility = new SpecialAbility
        {
            Modifiers = modifierList.ToImmutableList(),
            Name = rageMode.RageModeName,
            DecrementPeriod = rageMode.DecrementPeriod,
            Duration = rageMode.BoostDuration,
            DecrementCount = rageMode.DecrementCount,
            DecrementDelay = rageMode.DecrementDelay,
            ProgressPerAction = rageMode.GameLogicTrigger.Action.Progress,
            ActivatorName = rageMode.GameLogicTrigger.Activator.Type,
            ActivatorRadius = rageMode.GameLogicTrigger.Activator.Radius,
        };
        DataCache.TranslationNames.Add(specialAbility.Name);
        DataCache.TranslationNames.Add("RageMode");
        DataCache.TranslationNames.UnionWith(specialAbility.Modifiers.Select(m => m.Name));
        DataCache.TranslationNames.Add(specialAbility.ActivatorName);
        return specialAbility;
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
            "demoWithoutStatsPrem" => ShipCategory.TestShip,
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
            "superShip" => ShipCategory.TechTree,
            "coopOnly" => ShipCategory.Disabled,
            "event" => ShipCategory.Disabled,
            _ => throw new InvalidOperationException("Ship category not recognized: " + wgCategory),
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
        var upgrades = new List<ShipUpgrade>();
        foreach ((string wgName, var upgrade) in wgShip.ShipUpgradeInfo.ConvertedUpgrades)
        {
            var newUpgrade = new ShipUpgrade
            {
                Name = wgName,
                Components = upgrade.Components.Select(entry => (FindModuleType(entry.Key, logger), entry.Value))
                    .Where(entry => entry.Item1 != ComponentType.None)
                    .ToImmutableDictionary(entry => entry.Item1, entry => entry.Value.ToImmutableArray()),
                UcType = FindModuleType(upgrade.UcType, logger),
                CanBuy = upgrade.CanBuy,
                NextShips = upgrade.NextShips.ToImmutableArray(),
                Prev = upgrade.Prev,
            };
            if (newUpgrade.UcType != ComponentType.FlightControl)
            {
                upgrades.Add(newUpgrade);
            }
        }

        var upgradeInfo = new UpgradeInfo
        {
            ShipUpgrades = upgrades.ToImmutableList(),
            CostCredits = wgShip.ShipUpgradeInfo.CostCr,
            CostGold = wgShip.ShipUpgradeInfo.CostGold,
            CostXp = wgShip.ShipUpgradeInfo.CostXp,
            CostSaleGold = wgShip.ShipUpgradeInfo.CostSaleGold,
            Value = wgShip.ShipUpgradeInfo.Value,
        };

        return upgradeInfo;
    }

    private static ImmutableDictionary<string, TurretModule> ProcessMainBattery(WgShip wgShip, UpgradeInfo upgradeInfo, ShiptoolShip? stShip, Dictionary<string, Modifier> modifiersDictionary)
    {
        var resultDictionary = new Dictionary<string, TurretModule>();
        Dictionary<string, WgMainBattery> artilleryModules = wgShip.ModulesArmaments.ModulesOfType<WgMainBattery>();

        foreach ((string key, WgMainBattery wgMainBattery) in artilleryModules)
        {
            string correspondingHull = FindHullForComponent(upgradeInfo, ComponentType.Artillery, key);

            var stHullModule = stShip?.GetHullModule(correspondingHull);
            var turretModule = new TurretModule
            {
                Sigma = wgMainBattery.SigmaCount,
                MaxRange = wgMainBattery.MaxDist,
                Guns = wgMainBattery.Guns.Select(entry => ConvertMainBatteryGun(entry.Value, entry.Key, wgMainBattery.TaperDist, stHullModule)).ToImmutableArray(),
                BurstModeAbility = ProcessBurstModeAbility(wgMainBattery.SwitchableModeArtilleryModule, modifiersDictionary, wgShip.Name),
            };

            DataCache.TranslationNames.UnionWith(turretModule.Guns.Select(gun => gun.Name).Distinct());

            var targetAntiAir = new AntiAirBuilder();
            AssignAurasToProperty(wgMainBattery.AntiAirAuras, targetAntiAir);
            if (targetAntiAir.LongRangeAura?.ConstantDps > 0)
            {
                turretModule.AntiAir = targetAntiAir.LongRangeAura;
            }

            resultDictionary[key] = turretModule;
        }

        return resultDictionary.ToImmutableDictionary();
    }

    private static string FindHullForComponent(UpgradeInfo upgradeInfo, ComponentType componentType, string componentKey)
    {
        return upgradeInfo.ShipUpgrades
            .Where(u => u.UcType == ComponentType.Hull)
            .First(u => u.Components[componentType].Contains(componentKey)).Components[ComponentType.Hull].First();
    }

    private static Gun ConvertMainBatteryGun(WgGun wgGun, string mainGunKey, double taperDist, ShiptoolHullModule? stHull)
    {
        if (stHull is null || !stHull.Angles.TryGetValue(mainGunKey, out decimal angle))
        {
            angle = wgGun.Position[0] < 3 ? 0 : 180;
        }

        return wgGun.ConvertData(taperDist, mainGunKey, angle);
    }

    private static ImmutableDictionary<string, Hull> ProcessShipHull(WgShip wgShip, UpgradeInfo upgradeInfo)
    {
        var resultDictionary = new Dictionary<string, Hull>();
        Dictionary<string, WgHull> hullModules = wgShip.ModulesArmaments.ModulesOfType<WgHull>();

        foreach ((string key, WgHull wgHull) in hullModules)
        {
            ShipUpgrade hullUpgradeInfo = upgradeInfo.ShipUpgrades.First(upgradeEntry =>
                upgradeEntry.Components.ContainsKey(ComponentType.Hull) && upgradeEntry.Components[ComponentType.Hull].Contains(key));

            // Initialize basic hull data.
            var hullModule = new HullBuilder
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
                HitLocation hl = new(ShipHitLocation.Casemate, wgHull.Cas.HlType, wgHull.Cas.RegeneratedHpPart, wgHull.Cas.MaxHp);
                hitLocations.Add(hl);
            }

            if (!wgHull.Bow.HlType.Equals(string.Empty))
            {
                HitLocation hl = new(ShipHitLocation.Bow, wgHull.Bow.HlType, wgHull.Bow.RegeneratedHpPart, wgHull.Bow.MaxHp);
                hitLocations.Add(hl);
            }

            if (!wgHull.Ss.HlType.Equals(string.Empty))
            {
                HitLocation hl = new(ShipHitLocation.Superstructure, wgHull.Ss.HlType, wgHull.Ss.RegeneratedHpPart, wgHull.Ss.MaxHp);
                hitLocations.Add(hl);
            }

            if (!wgHull.St.HlType.Equals(string.Empty))
            {
                HitLocation hl = new(ShipHitLocation.Stern, wgHull.St.HlType, wgHull.St.RegeneratedHpPart, wgHull.St.MaxHp);
                hitLocations.Add(hl);
            }

            if (!wgHull.Ssc.HlType.Equals(string.Empty))
            {
                HitLocation hl = new(ShipHitLocation.AuxiliaryRooms, wgHull.Ssc.HlType, wgHull.Ssc.RegeneratedHpPart, wgHull.Ssc.MaxHp);
                hitLocations.Add(hl);
            }

            if (!wgHull.Hull.HlType.Equals(string.Empty))
            {
                HitLocation hl = new(ShipHitLocation.Hull, wgHull.Hull.HlType, wgHull.Hull.RegeneratedHpPart, wgHull.Hull.MaxHp);
                hitLocations.Add(hl);
            }

            if (!wgHull.Cit.HlType.Equals(string.Empty))
            {
                HitLocation hl = new(ShipHitLocation.Citadel, wgHull.Cit.HlType, wgHull.Cit.RegeneratedHpPart, wgHull.Cit.MaxHp);
                hitLocations.Add(hl);
            }

            hullModule.HitLocations = hitLocations.ToImmutableList();

            //process subs only parameters
            Dictionary<SubmarineBuoyancyStates, decimal> maxSpeedAtBuoyancyStateCoeff = new();
            if (ProcessShipClass(wgShip.TypeInfo.Species) == ShipClass.Submarine)
            {
                hullModule.DiveSpeed = wgHull.MaxBuoyancySpeed;
                hullModule.DivingPlaneShiftTime = wgHull.BuoyancyRudderTime / 1.305M;
                hullModule.SubBatteryCapacity = (decimal)wgHull.SubmarineBattery.Capacity;
                hullModule.SubBatteryRegenRate = (decimal)wgHull.SubmarineBattery.RegenRate;
                foreach ((string state, object[] coeff) in wgHull.BuoyancyStates)
                {
                    var depth = state switch
                    {
                        "DEEP_WATER_INVUL" => SubmarineBuoyancyStates.DeepWater,
                        "PERISCOPE" => SubmarineBuoyancyStates.Periscope,
                        "SURFACE" => SubmarineBuoyancyStates.Surface,
                        _ => throw new InvalidOperationException("Buoyancy state not recognized: " + wgHull),
                    };
                    maxSpeedAtBuoyancyStateCoeff.Add(depth, (decimal)(double)coeff[1]);
                }

                hullModule.MaxSpeedAtBuoyancyStateCoeff = maxSpeedAtBuoyancyStateCoeff.ToImmutableDictionary();
            }

            //Process ship size
            ShipSize shipSize = new(wgHull.Size[0], wgHull.Size[1], wgHull.Size[2]);
            hullModule.Sizes = shipSize;

            // Process anti-air data
            var antiAir = new AntiAirBuilder();

            var components = hullUpgradeInfo.Components[ComponentType.Secondary];
            if (components.Length > 0)
            {
                var wgHullSecondary = (WgAtba)wgShip.ModulesArmaments[components[0]];
                AssignAurasToProperty(wgHullSecondary.AntiAirAuras, antiAir);

                // Process secondaries
                var secondary = new TurretModule
                {
                    Sigma = wgHullSecondary.SigmaCount,
                    MaxRange = wgHullSecondary.MaxDist,
                    Guns = wgHullSecondary.AntiAirAndSecondaries.Values.Select(secondaryGun => secondaryGun.ConvertData(wgHullSecondary.TaperDist, string.Empty, default)).ToImmutableArray(),
                };

                DataCache.TranslationNames.UnionWith(secondary.Guns.Select(gun => gun.Name).Distinct());
                hullModule.SecondaryModule = secondary;
            }

            if (hullUpgradeInfo.Components.TryGetValue(ComponentType.AirDefense, out ImmutableArray<string> airDefenseKeys))
            {
                foreach (string airDefenseKey in airDefenseKeys)
                {
                    var airDefenseArmament = (WgAirDefense)wgShip.ModulesArmaments[airDefenseKey];
                    AssignAurasToProperty(airDefenseArmament.AntiAirAuras, antiAir);
                }
            }

            hullModule.AntiAir = antiAir.Build();

            // Process depth charge data
            if (hullUpgradeInfo.Components.TryGetValue(ComponentType.DepthCharges, out ImmutableArray<string> depthChargeKey) && depthChargeKey.Length > 0)
            {
                var wgDepthChargeArray = (WgDepthChargesArray)wgShip.ModulesArmaments[depthChargeKey[0]];
                hullModule.DepthChargeArray = new DepthChargeArray
                {
                    MaxPacks = wgDepthChargeArray.MaxPacks,
                    NumShots = wgDepthChargeArray.NumShots,
                    Reload = wgDepthChargeArray.ReloadTime,
                    DepthCharges = wgDepthChargeArray.DepthCharges.Select(entry => entry.Value.ConvertData()).ToImmutableArray(),
                };
                DataCache.TranslationNames.UnionWith(hullModule.DepthChargeArray.DepthCharges.Select(depthChargeLauncher => depthChargeLauncher.Name)
                    .Distinct());
            }

            resultDictionary[key] = hullModule.Build();
        }

        return resultDictionary.ToImmutableDictionary();
    }

    private static ImmutableDictionary<string, FireControl> ProcessFireControl(WgShip wgShip)
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

        return resultDictionary.ToImmutableDictionary();
    }

    private static ImmutableDictionary<string, TorpedoModule> ProcessTorpedoes(WgShip wgShip, UpgradeInfo upgradeInfo, ShiptoolShip? stShip)
    {
        var resultDictionary = new Dictionary<string, TorpedoModule>();
        Dictionary<string, WgTorpedoArray> wgTorpedoList = wgShip.ModulesArmaments.ModulesOfType<WgTorpedoArray>();

        foreach ((string key, WgTorpedoArray wgTorpedoArray) in wgTorpedoList)
        {
            string correspondingHull = FindHullForComponent(upgradeInfo, ComponentType.Torpedoes, key);
            var stHull = stShip?.GetHullModule(correspondingHull);
            var launchers = wgTorpedoArray.TorpedoArray.Select(entry => ConvertWgTorpedoLauncher(entry.Key, entry.Value, stHull)).ToImmutableArray();

            // process launcher loaders
            ImmutableDictionary<SubmarineTorpedoLauncherLoaderPosition, ImmutableArray<string>> torpedoLoaders = ImmutableDictionary<SubmarineTorpedoLauncherLoaderPosition, ImmutableArray<string>>.Empty;
            if (wgTorpedoArray.Groups.Count > 0 && wgTorpedoArray.Loaders.Count > 0)
            {
                torpedoLoaders = ConvertLoaders(wgTorpedoArray, launchers);
            }

            var torpedoModule = new TorpedoModule
            {
                TorpedoLaunchers = launchers,
                TorpedoLoaders = torpedoLoaders,
            };
            DataCache.TranslationNames.UnionWith(torpedoModule.TorpedoLaunchers.Select(launcher => launcher.Name).Distinct());

            resultDictionary[key] = torpedoModule;
        }

        return resultDictionary.ToImmutableDictionary();
    }

    private static ImmutableDictionary<SubmarineTorpedoLauncherLoaderPosition, ImmutableArray<string>> ConvertLoaders(WgTorpedoArray wgTorpedoArray, ImmutableArray<TorpedoLauncher> launchers)
    {
        Dictionary<int, int> bowLoaders = new();
        Dictionary<int, int> sternLoaders = new();
        foreach (var jToken in wgTorpedoArray.Groups)
        {
            var groupName = jToken.First!.ToObject<int>();
            var group = jToken.Last!.ToObject<string[]>()!;

            // since it is a group we can assume they are all close together so we can by checking only 1 element of the group we can address all of it
            var launcher = launchers.First(x => x.GroupName.Equals(group[0]));
            if (launcher.BaseAngle is <= 90 or >= 270)
            {
                // if the launcher base angle is between 90° and -90° we cam assume it is looking forward and so positioned in the front
                bowLoaders.Add(groupName, group.Length);
            }
            else
            {
                sternLoaders.Add(groupName, group.Length);
            }
        }

        Dictionary<int, int> loaders = wgTorpedoArray.Loaders.ToDictionary(jToken => jToken.Last!.ToObject<int[]>()!.Single(), jToken => jToken.First!.ToObject<int>());
        Dictionary<string, int> bowLoadersAmounts = new();
        Dictionary<string, int> sternLoadersAmounts = new();
        foreach (var bowLoader in bowLoaders)
        {
            var tubesPerLoader = loaders.Where(x => x.Key.Equals(bowLoader.Key)).Select(x => x.Value).Single();
            var setup = $"{bowLoader.Value / tubesPerLoader}x{tubesPerLoader}";

            // Disable CA1854 since TryGetValue would not modify the value in the dictionary
#pragma warning disable CA1854
            if (bowLoadersAmounts.ContainsKey(setup))
            {
                bowLoadersAmounts[setup]++;
            }
            else
            {
                bowLoadersAmounts.Add(setup, 1);
            }
        }

        foreach (var sternLoader in sternLoaders)
        {
            var tubesPerLoader = loaders.Where(x => x.Key.Equals(sternLoader.Key)).Select(x => x.Value).Single();
            var setup = $"{sternLoader.Value / tubesPerLoader}x{tubesPerLoader}";
            if (sternLoadersAmounts.ContainsKey(setup))
            {
                sternLoadersAmounts[setup]++;
            }
            else
            {
                sternLoadersAmounts.Add(setup, 1);
            }
        }
#pragma warning restore CA1854

        List<string> bowLoadersSetup = new();
        List<string> sternLoadersSetup = new();
        foreach (var bowLoadersAmount in bowLoadersAmounts)
        {
            string setup;
            if (bowLoadersAmount.Value > 1)
            {
                var t = bowLoadersAmount.Key.Split('x');
                setup = $"{int.Parse(t[0], CultureInfo.InvariantCulture) * bowLoadersAmount.Value}x{t[1]}";
            }
            else
            {
                setup = bowLoadersAmount.Key;
            }

            bowLoadersSetup.Add(setup);
        }

        foreach (var sternLoadersAmount in sternLoadersAmounts)
        {
            string setup;
            if (sternLoadersAmount.Value > 1)
            {
                var t = sternLoadersAmount.Key.Split('x');
                setup = $"{int.Parse(t[0], CultureInfo.InvariantCulture) * sternLoadersAmount.Value}x{t[1]}";
            }
            else
            {
                setup = sternLoadersAmount.Key;
            }

            sternLoadersSetup.Add(setup);
        }

        Dictionary<SubmarineTorpedoLauncherLoaderPosition, ImmutableArray<string>> torpedoLoaders = new()
        {
            { SubmarineTorpedoLauncherLoaderPosition.BowLoaders, bowLoadersSetup.ToImmutableArray() },
            { SubmarineTorpedoLauncherLoaderPosition.SternLoaders, sternLoadersSetup.ToImmutableArray() },
        };

        return torpedoLoaders.ToImmutableDictionary();
    }

    private static TorpedoLauncher ConvertWgTorpedoLauncher(string wgKey, WgTorpedoLauncher wgTorpedoLauncher, ShiptoolHullModule? stHull)
    {
        if (stHull is null || !stHull.Angles.TryGetValue(wgKey, out decimal angle))
        {
            angle = wgTorpedoLauncher.Position[0] < 3 ? 0 : 180;
        }

        return wgTorpedoLauncher.ConvertData(wgKey, angle);
    }

    private static ImmutableDictionary<string, Engine> ProcessEngines(WgShip wgShip)
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

        return resultDictionary.ToImmutableDictionary();
    }

    private static ImmutableDictionary<string, ImmutableArray<string>> ProcessPlanes(WgShip wgShip, UpgradeInfo upgradeInfo)
    {
        var validPlaneTypes = new List<ComponentType> { ComponentType.Fighter, ComponentType.TorpedoBomber, ComponentType.DiveBomber, ComponentType.SkipBomber };
        IEnumerable<ShipUpgrade> planeUpgrades = upgradeInfo.ShipUpgrades.Where(upgrade => validPlaneTypes.Contains(upgrade.UcType));
        IEnumerable<string> planeModuleNames = planeUpgrades.Select(u => u.Components[u.UcType][0]);
        Dictionary<string, ImmutableArray<string>> planeModules = planeModuleNames
            .Select(module => (module, (WgPlane)wgShip.ModulesArmaments[module]))
            .ToDictionary(x => x.module, x => ExtractPlaneList(x.Item2));

        IEnumerable<string> planeNames = planeModules.SelectMany(x => x.Value).Distinct();
        DataCache.TranslationNames.UnionWith(planeNames);

        return planeModules.ToImmutableDictionary();
    }

    private static ImmutableArray<string> ExtractPlaneList(WgPlane wgPlane)
    {
        // Process ships starting with patch 0.11.1
        if (wgPlane.Planes != null)
        {
            return wgPlane.Planes.ToImmutableArray();
        }

        // Processing for older versions
        return ImmutableArray.Create(wgPlane.PlaneType ?? string.Empty);
    }

    private static ImmutableList<ShipConsumable> ProcessConsumables(WgShip ship)
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

        return resultList.ToImmutableList();
    }

    private static ImmutableDictionary<string, AirStrike> ProcessAirstrikes(WgShip wgShip)
    {
        Dictionary<string, AirStrike> result = wgShip.ModulesArmaments
            .ModulesOfType<WgAirSupport>()
            .ToDictionary(entry => entry.Key, entry => entry.Value.ConvertData());
        DataCache.TranslationNames.UnionWith(result.Values.Select(airStrike => airStrike.PlaneName).Distinct());
        return result.ToImmutableDictionary();
    }

    private static ImmutableDictionary<string, PingerGun> ProcessPingerGuns(WgShip wgShip)
    {
        return wgShip.ModulesArmaments
            .ModulesOfType<WgPingerGun>()
            .ToImmutableDictionary(entry => entry.Key, entry => entry.Value.ConvertData());
    }

    #endregion

    private static Dictionary<string, T> ModulesOfType<T>(this Dictionary<string, WgArmamentModule> thisDict) where T : WgArmamentModule
    {
        return thisDict.Where(module => module.Value is T)
            .ToDictionary(entry => entry.Key, entry => (T)entry.Value);
    }

    private static void AssignAurasToProperty(Dictionary<string, WgAaAura>? auras, AntiAirBuilder? targetAntiAir)
    {
        if (auras == null || targetAntiAir == null)
        {
            return;
        }

        foreach (var (_, aura) in auras)
        {
            switch (aura.Type)
            {
                case "far":
                    targetAntiAir.LongRangeAura = targetAntiAir.LongRangeAura != null ? targetAntiAir.LongRangeAura.AddAura(aura.ConvertData()) : aura.ConvertData();
                    break;
                case "medium":
                    targetAntiAir.MediumRangeAura = targetAntiAir.MediumRangeAura != null ? targetAntiAir.MediumRangeAura.AddAura(aura.ConvertData()) : aura.ConvertData();
                    break;
                case "near":
                    targetAntiAir.ShortRangeAura = targetAntiAir.ShortRangeAura != null ? targetAntiAir.ShortRangeAura.AddAura(aura.ConvertData()) : aura.ConvertData();
                    break;
            }
        }
    }

    private static ComponentType FindModuleType(string rawModuleType, ILogger? logger)
    {
        rawModuleType = rawModuleType.TrimStart('_');
        string normalizedInput = rawModuleType.First().ToString().ToUpper(CultureInfo.InvariantCulture) + rawModuleType[1..];
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
            "Pinger" => ComponentType.Sonar, // for UcType, Sonar is used while pinger is used in components list
            _ => ComponentType.None,
        };

        if (componentType == ComponentType.None && !ReportedTypes.Contains(normalizedInput))
        {
            ReportedTypes.Add(normalizedInput);
            logger?.LogWarning("Cannot find type for provided string: {NormalizedInput}", normalizedInput);
        }

        return componentType;
    }

    private static ImmutableDictionary<string, ShellCompatibility> CheckShellCompatibilities(ImmutableDictionary<string, TurretModule> mainBatteries, UpgradeInfo upgradeInfo)
    {
        var shells = mainBatteries.SelectMany(pair => pair.Value.Guns.FirstOrDefault()?.AmmoList ?? ImmutableArray<string>.Empty).ToList();
        if (shells.Count == 0)
        {
            return ImmutableDictionary<string, ShellCompatibility>.Empty;
        }

        return shells.Distinct().ToImmutableDictionary(shell => shell, shell => CheckShellCompatibility(shell, mainBatteries, upgradeInfo));
    }

    private static ShellCompatibility CheckShellCompatibility(string shellName, ImmutableDictionary<string, TurretModule> mainBatteries, UpgradeInfo upgradeInfo)
    {
        var compatibleArtilleryModules = mainBatteries
            .Where(pair => pair.Value.Guns[0].AmmoList.Contains(shellName))
            .Select(pair => pair.Key);
        var compatibleModulesCombo = upgradeInfo.ShipUpgrades
            .Where(upgrade => upgrade.UcType == ComponentType.Hull)
            .Where(upgrade => upgrade.Components[ComponentType.Artillery].Any(c => compatibleArtilleryModules.Contains(c)))
            .OrderBy(item => item, UpgradeComparer.Instance)
            .ToDictionary(hullUpgrade => hullUpgrade.Components[ComponentType.Hull].Single(), artilleryUpgrade => artilleryUpgrade.Components[ComponentType.Artillery].Intersect(compatibleArtilleryModules).ToImmutableList());

        return new(shellName, compatibleModulesCombo.ToImmutableDictionary());
    }

    private sealed class UpgradeComparer : IComparer<ShipUpgrade>
    {
        public static UpgradeComparer Instance { get; } = new();

        public int Compare(ShipUpgrade? firstUpgrade, ShipUpgrade? secondUpgrade)
        {
            if (firstUpgrade == null || secondUpgrade == null)
            {
                return 0;
            }

            if (firstUpgrade.Prev == secondUpgrade.Prev)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(firstUpgrade.Prev))
            {
                return -1;
            }

            if (string.IsNullOrEmpty(secondUpgrade.Prev))
            {
                return 1;
            }

            if (firstUpgrade.Prev == secondUpgrade.Name)
            {
                return 1;
            }

            if (secondUpgrade.Prev == firstUpgrade.Name)
            {
                return -1;
            }

            return 0;
        }
    }

    private sealed class AntiAirBuilder
    {
        public AntiAirAura? LongRangeAura { get; set; }

        public AntiAirAura? MediumRangeAura { get; set; }

        public AntiAirAura? ShortRangeAura { get; set; }

        public AntiAir Build()
        {
            return new()
            {
                LongRangeAura = LongRangeAura,
                MediumRangeAura = MediumRangeAura,
                ShortRangeAura = ShortRangeAura,
            };
        }
    }

    private sealed class HullBuilder
    {
        public decimal Health { get; set; }

        public decimal MaxSpeed { get; set; }

        public decimal RudderTime { get; set; }

        public decimal SpeedCoef { get; set; }

        public decimal SteeringGearArmorCoeff { get; set; }

        public decimal SmokeFiringDetection { get; set; }

        public decimal SurfaceDetection { get; set; }

        public decimal AirDetection { get; set; }

        public decimal DetectionBySubPeriscope { get; set; }

        public decimal DetectionBySubOperating { get; set; }

        public AntiAir? AntiAir { get; set; }

        public TurretModule? SecondaryModule { get; set; }

        public DepthChargeArray? DepthChargeArray { get; set; }

        public int FireSpots { get; set; }

        public decimal FireResistance { get; set; }

        public decimal FireTickDamage { get; set; }

        public decimal FireDuration { get; set; }

        public int FloodingSpots { get; set; }

        public decimal FloodingResistance { get; set; }

        public decimal FloodingTickDamage { get; set; }

        public decimal FloodingDuration { get; set; }

        public int TurningRadius { get; set; }

        public ShipSize Sizes { get; set; } = new(default, default, default);

        public int EnginePower { get; set; }

        public int Tonnage { get; set; }

        public ImmutableList<HitLocation> HitLocations { get; set; } = ImmutableList<HitLocation>.Empty;

        public ImmutableDictionary<SubmarineBuoyancyStates, decimal> MaxSpeedAtBuoyancyStateCoeff { get; set; } = ImmutableDictionary<SubmarineBuoyancyStates, decimal>.Empty;

        public decimal SubBatteryCapacity { get; set; }

        public decimal SubBatteryRegenRate { get; set; }

        public decimal DiveSpeed { get; set; }

        public decimal DivingPlaneShiftTime { get; set; }

        public Hull Build()
        {
            return new Hull
            {
                Health = Health,
                MaxSpeed = MaxSpeed,
                RudderTime = RudderTime,
                SpeedCoef = SpeedCoef,
                SteeringGearArmorCoeff = SteeringGearArmorCoeff,
                SmokeFiringDetection = SmokeFiringDetection,
                SurfaceDetection = SurfaceDetection,
                AirDetection = AirDetection,
                DetectionBySubPeriscope = DetectionBySubPeriscope,
                DetectionBySubOperating = DetectionBySubOperating,
                AntiAir = AntiAir,
                SecondaryModule = SecondaryModule,
                DepthChargeArray = DepthChargeArray,
                FireSpots = FireSpots,
                FireResistance = FireResistance,
                FireTickDamage = FireTickDamage,
                FireDuration = FireDuration,
                FloodingSpots = FloodingSpots,
                FloodingResistance = FloodingResistance,
                FloodingTickDamage = FloodingTickDamage,
                FloodingDuration = FloodingDuration,
                TurningRadius = TurningRadius,
                Sizes = Sizes,
                EnginePower = EnginePower,
                Tonnage = Tonnage,
                HitLocations = HitLocations,
                MaxSpeedAtBuoyancyStateCoeff = MaxSpeedAtBuoyancyStateCoeff,
                SubBatteryCapacity = SubBatteryCapacity,
                SubBatteryRegenRate = SubBatteryRegenRate,
                DiveSpeed = DiveSpeed,
                DivingPlaneShiftTime = DivingPlaneShiftTime,
            };
        }
    }
}
