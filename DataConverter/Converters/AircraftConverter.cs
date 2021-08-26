using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WoWsShipBuilderDataStructures;

namespace DataConverter.Converters
{
    public class AircraftConverter
    {
        //convert the list of modernizations from WG to our list of Modernizations
        public static Dictionary<string, Aircraft> ConvertAircraft(string jsonInput)
        {
            //create a List of our Objects
            Dictionary<string, Aircraft> airList = new Dictionary<string, Aircraft>();

            //deserialize into an object
            var wgAircraft = JsonConvert.DeserializeObject<List<WGAircraft>>(jsonInput) ?? throw new InvalidOperationException();

            //iterate over the entire list to convert everything
            foreach (var currentWgAir in wgAircraft)
            {
                //create our object type
                Aircraft air = new Aircraft()
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
                    DamageTakenMultiplier = currentWgAir.damageTakenMultiplier,
                    FlightHeight = currentWgAir.flightHeight,
                    FlightRadius = currentWgAir.flightRadius,
                    InnerBombsPercentage = currentWgAir.innerBombsPercentage,
                    InnerSalvoSize = currentWgAir.innerSalvoSize,
                };

                PlaneAttackData pad = new PlaneAttackData()
                {
                    //start mapping
                    AttackCooldown = currentWgAir.attackCooldown,
                    AttackCount = currentWgAir.attackCount,
                    AttackInterval = currentWgAir.attackInterval,
                    AttackSpeedMultiplier = currentWgAir.attackSpeedMultiplier,
                    AttackSpeedMultiplierApplyTime = currentWgAir.attackSpeedMultiplierApplyTime,
                    AttackerDamageTakenMultiplier = currentWgAir.attackerDamageTakenMultiplier,
                    AttackerSize = currentWgAir.attackerSize,
                };
                air.AttackData = pad;

                JatoData jd = new JatoData()
                {
                    //start mapping
                    JatoDuration = currentWgAir.jatoDuration,
                    JatoSpeedMultiplier = currentWgAir.jatoSpeedMultiplier
                };
                air.JatoData = jd;

                //determine the needed enum for planecategory
                if (currentWgAir.isConsumablePlane)
                {
                    air.PlaneCategory = PlaneCategory.Consumable;
                }

                if (currentWgAir.isAirSupportPlane)
                {
                    air.PlaneCategory = PlaneCategory.Airstrike;
                }

                if (currentWgAir.isConsumablePlane == false && currentWgAir.isAirSupportPlane == false)
                {
                    air.PlaneCategory = PlaneCategory.Cv;
                }

                // dictionary with index as key, for easier search
                airList.Add(currentWgAir.index, air);
            }

            return airList;
        }
    }
}