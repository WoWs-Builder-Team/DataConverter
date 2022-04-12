using GameParamsExtractor.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters
{
    public static class AircraftConverter
    {
        //convert the list of aircrafts from WG to our list of Aircrafts
        public static Dictionary<string, Aircraft> ConvertAircraft(IEnumerable<WgAircraft> wgAircraft)
        {
            //create a List of our Objects
            Dictionary<string, Aircraft> airList = new();

            //iterate over the entire list to convert everything
            foreach (var currentWgAir in wgAircraft)
            {
                Program.TranslationNames.Add(currentWgAir.Name);
                //create our object type
                Aircraft air = new()
                {
                    //start mapping
                    Id = currentWgAir.Id,
                    Index = currentWgAir.Index,
                    MaxHealth = currentWgAir.MaxHealth,
                    Name = currentWgAir.Name,
                    NumPlanesInSquadron = currentWgAir.NumPlanesInSquadron,
                    ReturnHeight = currentWgAir.ReturnHeight,
                    SpeedMaxModifier = currentWgAir.SpeedMax,
                    SpeedMinModifier = currentWgAir.SpeedMin,
                    Speed = currentWgAir.SpeedMoveWithBomb,
                    NaturalAcceleration = currentWgAir.NaturalAcceleration,
                    NaturalDeceleration = currentWgAir.NaturalDeceleration,
                    MaxPlaneInHangar = currentWgAir.HangarSettings.MaxValue,
                    StartingPlanes = currentWgAir.HangarSettings.StartValue,
                    RestorationTime = currentWgAir.HangarSettings.TimeToRestore,
                    BombFallingTime = currentWgAir.BombFallingTime,
                    BombName = currentWgAir.BombName,
                    DamageTakenMultiplier = currentWgAir.AttackerDamageTakenMultiplier,
                    FlightHeight = currentWgAir.FlightHeight,
                    FlightRadius = currentWgAir.FlightRadius,
                    InnerBombsPercentage = currentWgAir.InnerBombsPercentage,
                    InnerSalvoSize = currentWgAir.InnerSalvoSize,
                    AimingAccuracyDecreaseRate = currentWgAir.AimingAccuracyDecreaseRate,
                    AimingAccuracyIncreaseRate = currentWgAir.AimingAccuracyIncreaseRate,
                    AimingTime = currentWgAir.AimingTime,
                    PostAttackInvulnerabilityDuration = currentWgAir.PostAttackInvulnerabilityDuration,
                    PreparationAccuracyDecreaseRate = currentWgAir.PreparationAccuracyDecreaseRate,
                    PreparationAccuracyIncreaseRate = currentWgAir.PreparationAccuracyIncreaseRate,
                    PreparationTime = currentWgAir.PreparationTime,
                    ConcealmentFromShips = currentWgAir.VisibilityFactor,
                    ConcealmentFromPlanes = currentWgAir.VisibilityFactorByPlane,
                    SpottingOnShips = currentWgAir.VisionToShip,
                    SpottingOnPlanes = currentWgAir.VisionToPlane,
                    SpottingOnTorps = currentWgAir.VisionToTorpedo,
                };

                PlaneAttackData planeAttackData = new()
                {
                    //start mapping
                    AttackCooldown = currentWgAir.AttackCooldown,
                    AttackCount = currentWgAir.ProjectilesPerAttack,
                    AttackInterval = currentWgAir.AttackInterval,
                    AttackSpeedMultiplier = currentWgAir.AttackSpeedMultiplier,
                    AttackSpeedMultiplierApplyTime = currentWgAir.AttackSpeedMultiplierApplyTime,
                    AttackerSize = currentWgAir.AttackerSize,
                };
                air.AttackData = planeAttackData;

                JatoData jatodata = new()
                {
                    //start mapping
                    JatoDuration = currentWgAir.JatoDuration,
                    JatoSpeedMultiplier = currentWgAir.JatoSpeedMultiplier,
                };
                air.JatoData = jatodata;

                //determine the needed enum for plane category
                bool isAirSupport;
                bool isConsumable;
                if (currentWgAir.PlaneSubtype == null)
                {
                    isAirSupport = currentWgAir.IsAirSupportPlane == true;
                    isConsumable = currentWgAir.IsConsumablePlane == true;
                }
                else
                {
                    List<string> subtypes = currentWgAir.PlaneSubtype.Select(subtype => subtype.ToLowerInvariant()).ToList();
                    isAirSupport = subtypes.Contains("airsupport");
                    isConsumable = subtypes.Contains("consumable");
                }

                if (isConsumable && isAirSupport)
                {
                    air.PlaneCategory = PlaneCategory.Asw;
                }
                else if (isConsumable)
                {
                    air.PlaneCategory = PlaneCategory.Consumable;
                }
                else if (isAirSupport)
                {
                    air.PlaneCategory = PlaneCategory.Airstrike;
                }
                else
                {
                    air.PlaneCategory = PlaneCategory.Cv;
                }

                air.AircraftConsumable = ProcessConsumables(currentWgAir);

                // dictionary with index as key, for easier search
                airList.Add(currentWgAir.Index, air);
            }

            return airList;
        }

        private static List<AircraftConsumable> ProcessConsumables(WgAircraft aircraft)
        {
            var resultList = new List<AircraftConsumable>();
            foreach ((_, AircraftAbility wgAbility) in aircraft.PlaneAbilities)
            {
                IEnumerable<AircraftConsumable> consumableList = wgAbility.Abils
                    .Select(ability => (AbilityName: ability[0], AbilityVariant: ability[1]))
                    .Select(ability =>
                        new AircraftConsumable
                        {
                            ConsumableName = ability.AbilityName,
                            ConsumableVariantName = ability.AbilityVariant,
                            Slot = wgAbility.Slot,
                        });
                resultList.AddRange(consumableList);
            }

            return resultList;
        }
    }
}
