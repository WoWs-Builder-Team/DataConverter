using System;
using System.Collections.Generic;
using System.Linq;
using DataConverter.WGStructure;
using Newtonsoft.Json;
using WoWsShipBuilderDataStructures;
using Hull = WoWsShipBuilderDataStructures.Hull;
using ShipUpgrade = WoWsShipBuilderDataStructures.ShipUpgrade;

namespace DataConverter.Converters
{
    public static class ShipConverter
    {
        private static readonly HashSet<string> ReportedTypes = new();

        public static List<ShipSummary> ShipSummaries = new();

        public static Dictionary<string, Ship> ConvertShips(string jsonInput)
        {
            var results = new Dictionary<string, Ship>();

            List<WGShip> wgShipList = JsonConvert.DeserializeObject<List<WGShip>>(jsonInput) ?? throw new InvalidOperationException();

            Dictionary<string, string> shipToPreviousShipMapper = new Dictionary<string, string>();
            Dictionary<string, List<string>> shipToNextShipMapper = new Dictionary<string, List<string>>();

            foreach (WGShip wgShip in wgShipList)
            {
                if (wgShip.typeinfo.species.Equals(ShipClass.Auxiliary.ToString()) || wgShip.group.Equals("clan") || wgShip.group.Equals("disabled") ||
                    wgShip.group.Equals("preserved") || wgShip.group.Equals("unavailable"))
                {
                    continue;
                }

                Program.TranslationNames.Add(wgShip.index);
                var ship = new Ship
                {
                    Id = wgShip.id,
                    Index = wgShip.index,
                    Name = wgShip.name,
                    Tier = wgShip.level,
                    ShipClass = ProcessShipClass(wgShip.typeinfo.species),
                    ShipCategory = ProcessShipCategory(wgShip.group, wgShip.level),
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
                    BurstModeAbility = ProcessBurstModeAbility(wgShip.BurstArtilleryModule)
                };

                ship.Permaflages = wgShip.permoflages;
                if (wgShip.permoflages != null)
                {
                    Program.TranslationNames.UnionWith(wgShip.permoflages);
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
                shipToPreviousShipMapper.TryGetValue(ship.Index, out string previousShip);
                shipToNextShipMapper.TryGetValue(ship.Index, out List<string> nextShips);
                ShipSummaries.Add(new ShipSummary(ship.Index, ship.ShipNation, ship.Tier, ship.ShipClass, ship.ShipCategory, previousShip, nextShips));
            }

            return results;
        }

        #region Component converters

        private static BurstModeAbility ProcessBurstModeAbility(BurstArtilleryModule module)
        {
            if (module != null)
            {
                var burstAbility = new BurstModeAbility()
                {
                    ShotInBurst = module.shotsCount,
                    ReloadAfterBurst = module.fullReloadTime,
                    ReloadDuringBurst = module.burstReloadTime,
                    Modifiers = module.modifiers
                };
                Program.TranslationNames.UnionWith(burstAbility.Modifiers.Keys);
                return burstAbility;
            }
            return null;
        }

        private static SpecialAbility ProcessSpecialAbility(WGShip wgShip)
        {
            Dictionary<string, WgSpecialAbility> wgSpecialAbilityList = wgShip.ModulesArmaments.ModulesOfType<WgSpecialAbility>();
            if (wgSpecialAbilityList.Count > 1)
            {
                throw new InvalidOperationException($"Too many special abilities for ship {wgShip.index}");
            }
            else if (wgSpecialAbilityList.Count == 1)
            {
                var wgAbility = wgSpecialAbilityList.Values.First().RageMode;
                var specialAbility = new SpecialAbility()
                {
                    Duration = wgAbility.boostDuration,
                    Modifiers = wgAbility.modifiers,
                    Name = wgAbility.rageModeName,
                    RadiusForSuccessfulHits = wgAbility.radius,
                    RequiredHits = wgAbility.requiredHits
                };
                Program.TranslationNames.Add(specialAbility.Name);
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
                _ => throw new InvalidOperationException("Ship category not recognized: " + wgCategory)
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
                _ => throw new InvalidOperationException("Ship class not recognized.")
            };
        }

        private static UpgradeInfo ProcessUpgradeInfo(WGShip wgShip)
        {
            var upgradeInfo = new UpgradeInfo
            {
                ShipUpgrades = new List<ShipUpgrade>(),
                CostCredits = wgShip.ShipUpgradeInfo.costCR,
                CostGold = wgShip.ShipUpgradeInfo.costGold,
                CostXp = wgShip.ShipUpgradeInfo.costXP,
                CostSaleGold = wgShip.ShipUpgradeInfo.costSaleGold,
                Value = wgShip.ShipUpgradeInfo.value,
            };

            foreach ((string wgName, WGStructure.ShipUpgrade upgrade) in wgShip.ShipUpgradeInfo.ConvertedUpgrades)
            {
                var newUpgrade = new ShipUpgrade
                {
                    Name = wgName,
                    Components = upgrade.components.Select(entry => (FindModuleType(entry.Key), entry.Value))
                        .Where(entry => entry.Item1 != ComponentType.None)
                        .ToDictionary(entry => entry.Item1, entry => entry.Value),
                    UcType = FindModuleType(upgrade.ucType),
                    CanBuy = upgrade.canBuy,
                    NextShips = upgrade.nextShips,
                    Prev = upgrade.prev,
                };
                upgradeInfo.ShipUpgrades.Add(newUpgrade);
            }

            return upgradeInfo;
        }

        private static Dictionary<string, TurretModule> ProcessMainBattery(WGShip wgShip)
        {
            var resultDictionary = new Dictionary<string, TurretModule>();
            Dictionary<string, MainBattery> artilleryModules = wgShip.ModulesArmaments.ModulesOfType<MainBattery>();

            foreach ((string key, MainBattery wgMainBattery) in artilleryModules)
            {
                var turretModule = new TurretModule
                {
                    Sigma = wgMainBattery.sigmaCount,
                    MaxRange = wgMainBattery.maxDist,
                    Guns = wgMainBattery.guns.Select(entry => entry.Value).Select(entry => (Gun)entry).ToList(),
                };
                MainBatteryGun dispersionGun = wgMainBattery.guns.Values.First();
                var turretDispersion = new Dispersion
                {
                    IdealRadius = dispersionGun.idealRadius,
                    MinRadius = dispersionGun.minRadius,
                    IdealDistance = dispersionGun.idealDistance,
                    TaperDist = wgMainBattery.taperDist,
                    RadiusOnZero = dispersionGun.radiusOnZero,
                    RadiusOnDelim = dispersionGun.radiusOnDelim,
                    RadiusOnMax = dispersionGun.radiusOnMax,
                    Delim = dispersionGun.delim,
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
                AssignAurasToProperty(wgMainBattery.ConvertedAntiAirAuras, targetAntiAir);
                if (targetAntiAir.LongRangeAura?.ConstantDps > 0)
                {
                    turretModule.AntiAir = targetAntiAir.LongRangeAura;
                }

                resultDictionary[key] = turretModule;
            }

            return resultDictionary;
        }

        private static Dictionary<string, Hull> ProcessShipHull(WGShip wgShip, UpgradeInfo upgradeInfo)
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
                    Health = wgHull.health,
                    MaxSpeed = wgHull.maxSpeed,
                    RudderTime = wgHull.rudderTime,
                    SpeedCoef = wgHull.speedCoef,
                    SmokeFiringDetection = wgHull.visibilityCoefGKInSmoke,
                    SurfaceDetection = wgHull.visibilityFactor,
                    AirDetection = wgHull.visibilityFactorByPlane,
                    FireSpots = wgHull.burnNodes.Length,
                    FireResistance = wgHull.burnNodes[0][0],
                    FireTickDamage = wgHull.burnNodes[0][1],
                    FireDuration = wgHull.burnNodes[0][2],
                    FloodingSpots = wgHull.floodNodes.Length,
                    FloodingResistance = wgHull.floodNodes[0][0],
                    FloodingTickDamage = wgHull.floodNodes[0][1],
                    FloodingDuration = wgHull.floodNodes[0][2],
                    TurningRadius = wgHull.turningRadius,
                };

                // Process anti-air data
                var antiAir = new AntiAir();

                string[] components = hullUpgradeInfo.Components[ComponentType.Secondary];
                if (components.Length > 0)
                {
                    var wgHullSecondary = (ATBA)wgShip.ModulesArmaments[components.First()];
                    AssignAurasToProperty(wgHullSecondary.ConvertedAntiAirData, antiAir);

                    // Process secondaries
                    var secondary = new TurretModule
                    {
                        Sigma = wgHullSecondary.sigmaCount,
                        MaxRange = wgHullSecondary.maxDist,
                        Guns = wgHullSecondary.antiAirAndSecondaries.Values.Select(secondaryGun => (Gun)secondaryGun).ToList(),
                    };
                    Program.TranslationNames.UnionWith(secondary.Guns.Select(gun => gun.Name).Distinct());
                    hullModule.SecondaryModule = secondary;
                }

                if (hullUpgradeInfo.Components.TryGetValue(ComponentType.AirDefense, out string[] airDefenseKeys))
                {
                    foreach (string airDefenseKey in airDefenseKeys)
                    {
                        var airDefenseArmament = (AirDefense)wgShip.ModulesArmaments[airDefenseKey];
                        AssignAurasToProperty(airDefenseArmament.ConvertedAuras, antiAir);
                    }
                }

                hullModule.AntiAir = antiAir;

                // Process depth charge data
                if (hullUpgradeInfo.Components.TryGetValue(ComponentType.DepthCharges, out string[] depthChargeKey) && depthChargeKey.Length > 0)
                {
                    var wgDepthChargeArray = (WgDepthChargesArray)wgShip.ModulesArmaments[depthChargeKey.First()];
                    hullModule.DepthChargeArray = new DepthChargeArray
                    {
                        MaxPacks = wgDepthChargeArray.maxPacks,
                        Reload = wgDepthChargeArray.reloadTime,
                        DepthCharges = wgDepthChargeArray.depthCharges.Select(entry => (DepthChargeLauncher)entry.Value).ToList(),
                    };
                    Program.TranslationNames.UnionWith(hullModule.DepthChargeArray.DepthCharges.Select(depthChargeLauncher => depthChargeLauncher.Name)
                        .Distinct());
                }

                resultDictionary[key] = hullModule;
            }

            return resultDictionary;
        }

        private static Dictionary<string, FireControl> ProcessFireControl(WGShip wgShip)
        {
            var resultDictionary = new Dictionary<string, FireControl>();

            Dictionary<string, WgFireControl> fireControlModules = wgShip.ModulesArmaments.ModulesOfType<WgFireControl>();

            foreach ((string key, WgFireControl wgFireControl) in fireControlModules)
            {
                var fireControl = new FireControl
                {
                    MaxRangeModifier = wgFireControl.maxDistCoef,
                    SigmaModifier = wgFireControl.sigmaCountCoef,
                };
                resultDictionary[key] = fireControl;
            }

            return resultDictionary;
        }

        private static Dictionary<string, TorpedoModule> ProcessTorpedoes(WGShip wgShip)
        {
            var resultDictionary = new Dictionary<string, TorpedoModule>();
            Dictionary<string, WgTorpedoArray> wgTorpedoList = wgShip.ModulesArmaments.ModulesOfType<WgTorpedoArray>();

            foreach ((string key, WgTorpedoArray wgTorpedoArray) in wgTorpedoList)
            {
                var torpedoModule = new TorpedoModule
                {
                    TorpedoLaunchers = wgTorpedoArray.torpedoArray.Select(entry => (TorpedoLauncher)entry.Value).ToList(),
                };
                Program.TranslationNames.UnionWith(torpedoModule.TorpedoLaunchers.Select(launcher => launcher.Name).Distinct());

                resultDictionary[key] = torpedoModule;
            }

            return resultDictionary;
        }

        private static Dictionary<string, Engine> ProcessEngines(WGShip wgShip)
        {
            var resultDictionary = new Dictionary<string, Engine>();
            Dictionary<string, WgEngine> wgEngineList = wgShip.ModulesArmaments.ModulesOfType<WgEngine>();

            foreach ((string key, WgEngine wgEngine) in wgEngineList)
            {
                var engine = new Engine
                {
                    BackwardEngineUpTime = wgEngine.backwardEngineUpTime,
                    ForwardEngineUpTime = wgEngine.forwardEngineUpTime,
                    SpeedCoef = wgEngine.speedCoef,
                };
                resultDictionary[key] = engine;
            }

            return resultDictionary;
        }

        private static Dictionary<string, PlaneData> ProcessPlanes(WGShip wgShip, UpgradeInfo upgradeInfo)
        {
            var resultDictionary = new Dictionary<string, PlaneData>();
            foreach (PlaneType type in Enum.GetValues(typeof(PlaneType)))
            {
                IEnumerable<(string moduleKey, PlaneData)> planesOfType = upgradeInfo.ShipUpgrades
                    .Where(upgrade => upgrade.UcType == type.ToComponentType())
                    .Select(planeUpgrade => planeUpgrade.Components[type.ToComponentType()].First())
                    .Select(moduleKey => (moduleKey, (WgPlane)wgShip.ModulesArmaments[moduleKey]))
                    .Select(wgPlane => (wgPlane.moduleKey, new PlaneData
                    {
                        PlaneName = wgPlane.Item2.planeType,
                        PlaneType = type,
                    }))
                    .ToList();

                Program.TranslationNames.UnionWith(planesOfType.Select(planeEntry => planeEntry.Item2.PlaneName).Distinct());
                foreach ((string moduleName, PlaneData planeData) in planesOfType)
                {
                    resultDictionary[moduleName] = planeData;
                }
            }

            return resultDictionary;
        }

        private static List<ShipConsumable> ProcessConsumables(WGShip ship)
        {
            var resultList = new List<ShipConsumable>();
            foreach ((_, ShipAbility wgAbility) in ship.ShipAbilities)
            {
                IEnumerable<ShipConsumable> consumableList = wgAbility.abils
                    .Select(ability => (AbilityName: ability[0], AbilityVariant: ability[1]))
                    .Select(ability =>
                        new ShipConsumable
                        {
                            ConsumableName = ability.AbilityName,
                            ConsumableVariantName = ability.AbilityVariant,
                            Slot = wgAbility.slot,
                        });
                resultList.AddRange(consumableList);
            }

            return resultList;
        }

        private static Dictionary<string, AirStrike> ProcessAirstrikes(WGShip wgShip)
        {
            Dictionary<string, AirStrike> result = wgShip.ModulesArmaments
                .ModulesOfType<AirSupport>()
                .ToDictionary(entry => entry.Key, entry => (AirStrike)entry.Value);
            Program.TranslationNames.UnionWith(result.Values.Select(airStrike => airStrike.PlaneName).Distinct());
            return result;
        }

        private static Dictionary<string, PingerGun> ProcessPingerGuns(WGShip wgShip)
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

        private static void AssignAurasToProperty(Dictionary<string, AAAura> auras, AntiAir targetAntiAir)
        {
            if (auras != null && targetAntiAir != null)
            {
                foreach ((_, AAAura aura) in auras)
                {
                    switch (aura.type)
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