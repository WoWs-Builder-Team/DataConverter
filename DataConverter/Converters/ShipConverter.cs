using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DataConverter.WGStructure;
using Newtonsoft.Json;
using WoWsShipBuilder.DataStructures;
using Hull = WoWsShipBuilder.DataStructures.Hull;
using ShipUpgrade = WoWsShipBuilder.DataStructures.ShipUpgrade;

namespace DataConverter.Converters
{
    public static class ShipConverter
    {
        private static readonly HashSet<string> ReportedTypes = new();

        private static readonly Dictionary<string, ShipTurretOverride> TurretPositionOverrides = LoadTurretOverrides();

#pragma warning disable SA1401
        public static List<ShipSummary> ShipSummaries = new();
#pragma warning restore SA1401

        public static Dictionary<string, Ship> ConvertShips(string jsonInput)
        {
            var results = new Dictionary<string, Ship>();

            List<WgShip> wgShipList = JsonConvert.DeserializeObject<List<WgShip>>(jsonInput) ?? throw new InvalidOperationException();

            Dictionary<string, string> shipToPreviousShipMapper = new Dictionary<string, string>();
            Dictionary<string, List<string>> shipToNextShipMapper = new Dictionary<string, List<string>>();

            foreach (WgShip wgShip in wgShipList)
            {
                if (wgShip.typeinfo.species.Equals(ShipClass.Auxiliary.ToString()) || wgShip.Group.Equals("clan") || wgShip.Group.Equals("disabled") ||
                    wgShip.Group.Equals("preserved") || wgShip.Group.Equals("unavailable"))
                {
                    continue;
                }

                Program.TranslationNames.Add(wgShip.Index);
                var ship = new Ship
                {
                    Id = wgShip.Id,
                    Index = wgShip.Index,
                    Name = wgShip.Name,
                    Tier = wgShip.Level,
                    ShipClass = ProcessShipClass(wgShip.typeinfo.species),
                    ShipCategory = ProcessShipCategory(wgShip.Group, wgShip.Level),
                    ShipNation = ConvertNationString(wgShip.typeinfo.nation),
                    MainBatteryModuleList = ProcessMainBattery(wgShip),
                    ShipUpgradeInfo = ProcessUpgradeInfo(wgShip),
                    FireControlList = ProcessFireControl(wgShip),
                    TorpedoModules = ProcessTorpedoes(wgShip),
                    Engines = ProcessEngines(wgShip),
                    ShipConsumable = ProcessConsumables(wgShip),
                    AirStrikes = ProcessAirstrikes(wgShip),
                    PingerGunList = ProcessPingerGuns(wgShip),
                    SpecialAbility = ProcessSpecialAbility(wgShip),
                };

                ship.Permoflages = wgShip.Permoflages;
                if (wgShip.Permoflages != null)
                {
                    Program.TranslationNames.UnionWith(wgShip.Permoflages);
                }

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

        private static Dictionary<string, ShipTurretOverride> LoadTurretOverrides()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DataConverter.JsonData.TurretPositionOverrides.json") ??
                               throw new FileNotFoundException("Unable to locate embedded captain skill data.");
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<Dictionary<string, ShipTurretOverride>>(jsonReader)!;
        }

        private static BurstModeAbility? ProcessBurstModeAbility(BurstArtilleryModule? module)
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
                Program.TranslationNames.UnionWith(burstAbility.Modifiers.Keys);
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
                Program.TranslationNames.Add(specialAbility.Name);
                Program.TranslationNames.Add("RageMode");
                Program.TranslationNames.UnionWith(specialAbility.Modifiers.Keys);
                return specialAbility;
            }

            return null;
        }

        private static Nation ConvertNationString(string wgNation)
        {
            return wgNation.Replace("_", "") switch
            {
                "USA" => Nation.Usa,
                { } any => Enum.Parse<Nation>(any),
            };
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

        private static UpgradeInfo ProcessUpgradeInfo(WgShip wgShip)
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

            foreach ((string wgName, WGStructure.ShipUpgrade upgrade) in wgShip.ShipUpgradeInfo.ConvertedUpgrades)
            {
                var newUpgrade = new ShipUpgrade
                {
                    Name = wgName,
                    Components = upgrade.Components.Select(entry => (FindModuleType(entry.Key), entry.Value))
                        .Where(entry => entry.Item1 != ComponentType.None)
                        .ToDictionary(entry => entry.Item1, entry => entry.Value),
                    UcType = FindModuleType(upgrade.UcType),
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

        private static Dictionary<string, TurretModule> ProcessMainBattery(WgShip wgShip)
        {
            var resultDictionary = new Dictionary<string, TurretModule>();
            Dictionary<string, MainBattery> artilleryModules = wgShip.ModulesArmaments.ModulesOfType<MainBattery>();

            foreach ((string key, MainBattery wgMainBattery) in artilleryModules)
            {
                var turretModule = new TurretModule
                {
                    Sigma = wgMainBattery.SigmaCount,
                    MaxRange = wgMainBattery.MaxDist,
                    Guns = wgMainBattery.Guns.Select(entry => ConvertMainBatteryGun(entry.Value, key, entry.Key, wgShip.Index)).ToList(),
                    BurstModeAbility = ProcessBurstModeAbility(wgMainBattery.BurstArtilleryModule),
                };
                MainBatteryGun dispersionGun = wgMainBattery.Guns.Values.First();
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

                // Calculation according to https://www.reddit.com/r/WorldOfWarships/comments/l1dpzt/reverse_engineered_dispersion_ellipse_including/
                // double maxRange = decimal.ToDouble(turretModule.MaxRange) / 30;
                // double horizontalDispersion = maxRange * (turretDispersion.IdealRadius - turretDispersion.MinRadius) /
                //     turretDispersion.IdealDistance + turretDispersion.MinRadius;
                //
                // double delimDist = turretDispersion.Delim * maxRange;
                // double verticalCoeff = turretDispersion.RadiusOnDelim +
                //                        (turretDispersion.RadiusOnMax - turretDispersion.RadiusOnDelim) * (maxRange - delimDist) / (maxRange - delimDist);
                // double verticalDispersion = horizontalDispersion * verticalCoeff;
                //
                // var effectiveHorizontalDispersion = Convert.ToDecimal(horizontalDispersion * 30);
                // turretDispersion.MaximumHorizontalDispersion = Math.Round(effectiveHorizontalDispersion, 1);
                // var effectiveVerticalDispersion = Convert.ToDecimal(verticalDispersion * 30);
                // turretDispersion.MaximumVerticalDispersion = Math.Round(effectiveVerticalDispersion, 1);

                var maxRange = decimal.ToDouble(turretModule.MaxRange);
                turretDispersion.MaximumHorizontalDispersion = Math.Round(Convert.ToDecimal(turretDispersion.CalculateHorizontalDispersion(maxRange)), 1);
                turretDispersion.MaximumVerticalDispersion = Math.Round(Convert.ToDecimal(turretDispersion.CalculateVerticalDispersion(maxRange)), 1);

                turretModule.DispersionValues = turretDispersion;
                Program.TranslationNames.UnionWith(turretModule.Guns.Select(gun => gun.Name).Distinct());

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

        private static Gun ConvertMainBatteryGun(MainBatteryGun wgGun, string artilleryModuleKey, string mainGunKey, string shipIndex)
        {
            var newGun = (Gun)wgGun;
            newGun.WgGunIndex = mainGunKey;
            if (TurretPositionOverrides.TryGetValue(shipIndex, out var overrides))
            {
                if (overrides.ArtilleryTurretOverrides.TryGetValue(artilleryModuleKey, out var moduleOverrides) && moduleOverrides.TryGetValue(mainGunKey, out var turretOverride))
                {
                    newGun.TurretOrientation = turretOverride;
                }
            }

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
                    DetectionBySubOperating = wgHull.VisibilityFactorsBySubmarine["DEEP_WATER"],
                    FireSpots = wgHull.BurnNodes.Length,
                    FireResistance = wgHull.BurnNodes[0][0],
                    FireTickDamage = wgHull.BurnNodes[0][1],
                    FireDuration = wgHull.BurnNodes[0][2],
                    FloodingSpots = wgHull.FloodNodes.Length,
                    FloodingResistance = wgHull.FloodNodes[0][0],
                    FloodingTickDamage = wgHull.FloodNodes[0][1],
                    FloodingDuration = wgHull.FloodNodes[0][2],
                    TurningRadius = wgHull.TurningRadius,
                };

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
                    var wgHullSecondary = (Atba)wgShip.ModulesArmaments[components.First()];
                    AssignAurasToProperty(wgHullSecondary.AntiAirAuras, antiAir);

                    // Process secondaries
                    var secondary = new TurretModule
                    {
                        Sigma = wgHullSecondary.SigmaCount,
                        MaxRange = wgHullSecondary.MaxDist,
                        Guns = wgHullSecondary.AntiAirAndSecondaries.Values.Select(secondaryGun => (Gun)secondaryGun).ToList(),
                    };
                    Program.TranslationNames.UnionWith(secondary.Guns.Select(gun => gun.Name).Distinct());
                    hullModule.SecondaryModule = secondary;
                }

                if (hullUpgradeInfo.Components.TryGetValue(ComponentType.AirDefense, out string[]? airDefenseKeys))
                {
                    foreach (string airDefenseKey in airDefenseKeys)
                    {
                        var airDefenseArmament = (AirDefense)wgShip.ModulesArmaments[airDefenseKey];
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
                        DepthCharges = wgDepthChargeArray.DepthCharges.Select(entry => (DepthChargeLauncher)entry.Value).ToList(),
                    };
                    Program.TranslationNames.UnionWith(hullModule.DepthChargeArray.DepthCharges.Select(depthChargeLauncher => depthChargeLauncher.Name)
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

        private static Dictionary<string, TorpedoModule> ProcessTorpedoes(WgShip wgShip)
        {
            var resultDictionary = new Dictionary<string, TorpedoModule>();
            Dictionary<string, WgTorpedoArray> wgTorpedoList = wgShip.ModulesArmaments.ModulesOfType<WgTorpedoArray>();

            foreach ((string key, WgTorpedoArray wgTorpedoArray) in wgTorpedoList)
            {
                var torpedoModule = new TorpedoModule
                {
                    TimeToChangeAmmo = wgTorpedoArray.TorpedoArray.Values.Any(launcher => launcher.AmmoList.Length > 1) ? wgTorpedoArray.TimeToChangeAmmo : 0,
                    TorpedoLaunchers = wgTorpedoArray.TorpedoArray.Select(entry => (TorpedoLauncher)entry.Value).ToList(),
                };
                Program.TranslationNames.UnionWith(torpedoModule.TorpedoLaunchers.Select(launcher => launcher.Name).Distinct());

                resultDictionary[key] = torpedoModule;
            }

            return resultDictionary;
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

                Program.TranslationNames.UnionWith(planesOfType.Select(planeEntry => planeEntry.Value.PlaneName).Distinct());
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
            foreach ((_, ShipAbility wgAbility) in ship.ShipAbilities)
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
                .ModulesOfType<AirSupport>()
                .ToDictionary(entry => entry.Key, entry => (AirStrike)entry.Value);
            Program.TranslationNames.UnionWith(result.Values.Select(airStrike => airStrike.PlaneName).Distinct());
            return result;
        }

        private static Dictionary<string, PingerGun> ProcessPingerGuns(WgShip wgShip)
        {
            return wgShip.ModulesArmaments
                .ModulesOfType<WgPingerGun>()
                .ToDictionary(entry => entry.Key, entry => (PingerGun)entry.Value);
        }

        #endregion

        #region Helper methods

        private static Dictionary<string, T> ModulesOfType<T>(this Dictionary<string, ModuleArmaments> thisDict) where T : ModuleArmaments
        {
            return thisDict.Where(module => module.Value is T)
                .ToDictionary(entry => entry.Key, entry => (T)entry.Value);
        }

        private static void AssignAurasToProperty(Dictionary<string, AaAura>? auras, AntiAir? targetAntiAir)
        {
            if (auras != null && targetAntiAir != null)
            {
                foreach ((_, AaAura aura) in auras)
                {
                    switch (aura.Type)
                    {
                        case "far":
                            if (targetAntiAir.LongRangeAura != null)
                            {
                                targetAntiAir.LongRangeAura += aura;
                            }
                            else
                            {
                                targetAntiAir.LongRangeAura = aura;
                            }

                            break;

                        case "medium":
                            if (targetAntiAir.MediumRangeAura != null)
                            {
                                targetAntiAir.MediumRangeAura += aura;
                            }
                            else
                            {
                                targetAntiAir.MediumRangeAura = aura;
                            }

                            break;

                        case "near":
                            if (targetAntiAir.ShortRangeAura != null)
                            {
                                targetAntiAir.ShortRangeAura += aura;
                            }
                            else
                            {
                                targetAntiAir.ShortRangeAura = aura;
                            }

                            break;
                    }
                }
            }
        }

        private static ComponentType FindModuleType(string rawModuleType)
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

            if (componentType == ComponentType.None)
            {
                if (ReportedTypes.Add(normalizedInput))
                {
                    Console.WriteLine($"Cannot find type for provided string: {normalizedInput}");
                }
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
                _ => throw new ArgumentOutOfRangeException(nameof(thisType), thisType, "Cannot process supplied plane type."),
            };
        }

        #endregion
    }
}
