using System;
using System.Collections.Generic;
using System.Linq;
using DataConverter.WGStructure;
using Newtonsoft.Json;
using WoWsShipBuilderDataStructures;
using Hull = WoWsShipBuilderDataStructures.Hull;

namespace DataConverter.Converters
{
    public static class ShipConverter
    {
        public static Dictionary<string, Ship> ConvertShips(string jsonInput)
        {
            var results = new Dictionary<string, Ship>();

            List<WGShip> wgShipList = JsonConvert.DeserializeObject<List<WGShip>>(jsonInput) ?? throw new InvalidOperationException();

            foreach (WGShip wgShip in wgShipList)
            {
                var ship = new Ship
                {
                    Id = wgShip.id,
                    Index = wgShip.index,
                    Name = wgShip.name,
                    Tier = wgShip.level,
                    MainBatteryModuleList = ProcessMainBattery(wgShip),
                    ShipUpgradeInfo = ProcessUpgradeInfo(wgShip),
                    FireControlList = ProcessFireControl(wgShip),
                    TorpedoModules = ProcessTorpedoes(wgShip),
                    Engines = ProcessEngines(wgShip),
                    ShipConsumable = ProcessConsumables(wgShip),
                    AirStrikes = ProcessAirstrikes(wgShip),
                    PingerGunList = ProcessPingerGuns(wgShip),
                };

                ship.Hulls = ProcessShipHull(wgShip, ship.ShipUpgradeInfo);
                ship.CvPlanes = ProcessPlanes(wgShip, ship.ShipUpgradeInfo);

                results[ship.Index] = ship;
            }

            return results;
        }

        #region Component converters

        private static UpgradeInfo ProcessUpgradeInfo(WGShip wgShip)
        {
            var upgradeInfo = new UpgradeInfo
            {
                ShipUpgrades = new List<ShipUpgrades>(),
                CostCredits = wgShip.ShipUpgradeInfo.costCR,
                CostGold = wgShip.ShipUpgradeInfo.costGold,
                CostXp = wgShip.ShipUpgradeInfo.costXP,
                CostSaleGold = wgShip.ShipUpgradeInfo.costSaleGold,
                Value = wgShip.ShipUpgradeInfo.value,
            };

            foreach ((_, ShipUpgrade upgrade) in wgShip.ShipUpgradeInfo.ConvertedUpgrades)
            {
                var newUpgrade = new ShipUpgrades
                {
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
                ShipUpgrades hullUpgradeInfo = upgradeInfo.ShipUpgrades.First(upgradeEntry =>
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
                };
                                
                // Process anti-air data
                var antiAir = new AntiAir
                {
                    LongRangeAura = new AntiAirAura(),
                    MediumRangeAura = new AntiAirAura(),
                    ShortRangeAura = new AntiAirAura(),
                };

                var components = hullUpgradeInfo.Components[ComponentType.Secondary];
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
                    }));

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
            return wgShip.ModulesArmaments
                .ModulesOfType<AirSupport>()
                .ToDictionary(entry => entry.Key, entry => (AirStrike)entry.Value);
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
                Console.WriteLine($"Cannot find type for provided string: {normalizedInput}");
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