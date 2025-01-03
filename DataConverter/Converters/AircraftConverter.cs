using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DataConverter.Data;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters
{
    public static class AircraftConverter
    {
        //convert the list of aircraft from WG to our list of aircraft
        public static Dictionary<string, Aircraft> ConvertAircraft(IEnumerable<WgAircraft> wgAircraft)
        {
            //create a List of our Objects
            Dictionary<string, Aircraft> airList = new();

            //iterate over the entire list to convert everything
            foreach (var currentWgAir in wgAircraft)
            {
                DataCache.TranslationNames.Add(currentWgAir.Name);
                PlaneAttackData planeAttackData = new()
                {
                    //start mapping
                    AttackCooldown = currentWgAir.AttackCooldown,
                    AttackCount = currentWgAir.AttackCount * currentWgAir.ProjectilesPerAttack,
                    AttackInterval = currentWgAir.AttackInterval,
                    AttackSpeedMultiplier = currentWgAir.AttackSpeedMultiplier,
                    AttackSpeedMultiplierApplyTime = currentWgAir.AttackSpeedMultiplierApplyTime,
                    AttackerSize = currentWgAir.AttackerSize,
                };

                //determine the needed enum for plane category
                bool isAirSupport;
                bool isConsumable;
                var isTactical = false;
                if (currentWgAir.PlaneSubtype.Length == 0)
                {
                    isAirSupport = currentWgAir.IsAirSupportPlane == true;
                    isConsumable = currentWgAir.IsConsumablePlane == true;
                }
                else
                {
                    List<string> subtypes = currentWgAir.PlaneSubtype.Select(subtype => subtype.ToLowerInvariant()).ToList();
                    isAirSupport = subtypes.Contains("airsupport");
                    isConsumable = subtypes.Contains("consumable");
                    isTactical = subtypes.Contains("jet") || subtypes.Contains("turboprop");
                }

                var planeCategory = isConsumable switch
                {
                    true when isAirSupport => PlaneCategory.Asw,
                    true => PlaneCategory.Consumable,
                    false when isAirSupport => PlaneCategory.Airstrike,
                    false => PlaneCategory.Cv,
                };

                var planeType = PlaneType.None;
                if (planeCategory == PlaneCategory.Cv)
                {
                    planeType = currentWgAir.TypeInfo.Species.ToLowerInvariant() switch
                    {
                        "fighter" => isTactical ? PlaneType.TacticalFighter : PlaneType.Fighter,
                        "bomber" => isTactical ? PlaneType.TacticalTorpedoBomber : PlaneType.TorpedoBomber,
                        "dive" => isTactical ? PlaneType.TacticalDiveBomber : PlaneType.DiveBomber,
                        "skip" => isTactical ? PlaneType.TacticalSkipBomber : PlaneType.SkipBomber,
                        _ => throw new InvalidOperationException("Invalid plane type detected"),
                    };
                }

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
                    MaxEngineBoostDuration = currentWgAir.MaxForsageAmount,
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
                    InnerSalvoSize = currentWgAir.InnerSalvoSize.Select(x => x * Constants.BigWorld).ToImmutableArray(),
                    OuterSalvoSize = currentWgAir.OuterSalvoSize.Select(x => x * Constants.BigWorld).ToImmutableArray(),
                    MinSpreadCoeff = (currentWgAir.MinSpread.Type.Equals(JTokenType.Array) ? currentWgAir.MinSpread.ToObject<float[]>()?.ToImmutableArray() : ImmutableArray.Create(currentWgAir.MinSpread.ToObject<float>())) ?? ImmutableArray<float>.Empty,
                    MaxSpreadCoeff = (currentWgAir.MaxSpread.Type.Equals(JTokenType.Array) ? currentWgAir.MaxSpread.ToObject<float[]>()?.ToImmutableArray() : ImmutableArray.Create(currentWgAir.MaxSpread.ToObject<float>())) ?? ImmutableArray<float>.Empty,
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
                    AttackData = planeAttackData,
                    JatoData = new(currentWgAir.JatoDuration, currentWgAir.JatoSpeedMultiplier),
                    AircraftConsumable = ProcessConsumables(currentWgAir),
                    PlaneCategory = planeCategory,
                    PlaneType = planeType,
                };

                // dictionary with index as key, for easier search
                airList.Add(currentWgAir.Index, air);
            }

            return airList;
        }

        private static ImmutableArray<AircraftConsumable> ProcessConsumables(WgAircraft aircraft)
        {
            var resultList = new List<AircraftConsumable>();
            foreach ((_, AircraftAbility wgAbility) in aircraft.PlaneAbilities)
            {
                IEnumerable<AircraftConsumable> consumableList = wgAbility.Abils
                    .Select(ability => (AbilityName: ability[0], AbilityVariant: ability[1]))
                    .Select(ability => new AircraftConsumable(wgAbility.Slot, ability.AbilityName, ability.AbilityVariant));
                resultList.AddRange(consumableList);
            }

            return resultList.ToImmutableArray();
        }
    }
}
