using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.DataStructures;

namespace DataConverter.Converters
{
    public static class AircraftConverter
    {
        //convert the list of aircrafts from WG to our list of Aircrafts
        public static Dictionary<string, Aircraft> ConvertAircraft(string jsonInput)
        {
            //create a List of our Objects
            Dictionary<string, Aircraft> airList = new Dictionary<string, Aircraft>();

            //deserialize into an object
            var wgAircraft = JsonConvert.DeserializeObject<List<WGAircraft>>(jsonInput) ?? throw new InvalidOperationException();

            //iterate over the entire list to convert everything
            foreach (var currentWgAir in wgAircraft)
            {
                Program.TranslationNames.Add(currentWgAir.name);
                //create our object type
                Aircraft air = new Aircraft
                {
                    //start mapping
                    Id = currentWgAir.id,
                    Index = currentWgAir.index,
                    MaxHealth = currentWgAir.maxHealth,
                    Name = currentWgAir.name,
                    NumPlanesInSquadron = currentWgAir.numPlanesInSquadron,
                    ReturnHeight = currentWgAir.returnHeight,
                    SpeedMaxModifier = currentWgAir.speedMax,
                    SpeedMinModifier = currentWgAir.speedMin,
                    Speed = currentWgAir.speedMoveWithBomb,
                    NaturalAcceleration = currentWgAir.naturalAcceleration,
                    NaturalDeceleration = currentWgAir.naturalDeceleration,
                    MaxPlaneInHangar = currentWgAir.hangarSettings.maxValue,
                    StartingPlanes = currentWgAir.hangarSettings.startValue,
                    RestorationTime = currentWgAir.hangarSettings.timeToRestore,
                    BombFallingTime = currentWgAir.bombFallingTime,
                    BombName = currentWgAir.bombName,
                    DamageTakenMultiplier = currentWgAir.attackerDamageTakenMultiplier,
                    FlightHeight = currentWgAir.flightHeight,
                    FlightRadius = currentWgAir.flightRadius,
                    InnerBombsPercentage = currentWgAir.innerBombsPercentage,
                    InnerSalvoSize = currentWgAir.innerSalvoSize,
                    AimingAccuracyDecreaseRate = currentWgAir.aimingAccuracyDecreaseRate,
                    AimingAccuracyIncreaseRate = currentWgAir.aimingAccuracyIncreaseRate,
                    AimingTime = currentWgAir.aimingTime,
                    PostAttackInvulnerabilityDuration = currentWgAir.postAttackInvulnerabilityDuration,
                    PreparationAccuracyDecreaseRate = currentWgAir.preparationAccuracyDecreaseRate,
                    PreparationAccuracyIncreaseRate = currentWgAir.preparationAccuracyIncreaseRate,
                    PreparationTime = currentWgAir.preparationTime,
                };

                PlaneAttackData planeAttackData = new PlaneAttackData
                {
                    //start mapping
                    AttackCooldown = currentWgAir.attackCooldown,
                    AttackCount = currentWgAir.projectilesPerAttack,
                    AttackInterval = currentWgAir.attackInterval,
                    AttackSpeedMultiplier = currentWgAir.attackSpeedMultiplier,
                    AttackSpeedMultiplierApplyTime = currentWgAir.attackSpeedMultiplierApplyTime,
                    AttackerSize = currentWgAir.attackerSize,
                };
                air.AttackData = planeAttackData;

                JatoData jatodata = new JatoData
                {
                    //start mapping
                    JatoDuration = currentWgAir.jatoDuration,
                    JatoSpeedMultiplier = currentWgAir.jatoSpeedMultiplier,
                };
                air.JatoData = jatodata;

                //determine the needed enum for plane category
                bool isAirSupport;
                bool isConsumable;
                if (currentWgAir.planeSubtype == null)
                {
                    isAirSupport = currentWgAir.isAirSupportPlane == true;
                    isConsumable = currentWgAir.isConsumablePlane == true;
                }
                else
                {
                    var subtypes = currentWgAir.planeSubtype.Select(subtype => subtype.ToLowerInvariant()).ToList();
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
                airList.Add(currentWgAir.index, air);
            }

            return airList;
        }

        private static List<AircraftConsumable> ProcessConsumables(WGAircraft aircraft)
        {
            var resultList = new List<AircraftConsumable>();
            foreach ((_, AircraftAbility wgAbility) in aircraft.PlaneAbilities)
            {
                IEnumerable<AircraftConsumable> consumableList = wgAbility.abils
                    .Select(ability => (AbilityName: ability[0], AbilityVariant: ability[1]))
                    .Select(ability =>
                        new AircraftConsumable
                        {
                            ConsumableName = ability.AbilityName,
                            ConsumableVariantName = ability.AbilityVariant,
                            Slot = wgAbility.slot,
                        });
                resultList.AddRange(consumableList);
            }

            return resultList;
        }
    }
}
