using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable NotAccessedPositionalProperty.Global

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace WoWsShipBuilder.DataStructures.Ship
{
#nullable enable

    public sealed record Ship(long Id, string Index, string Name, int Tier, ShipClass ShipClass, ShipCategory ShipCategory, Nation ShipNation)
    {
        public List<string> Permaflages { get; set; } = default!;

        public Dictionary<string, TurretModule> MainBatteryModuleList { get; set; } = default!;
        public Dictionary<string, FireControl> FireControlList { get; set; } = default!;
        public Dictionary<string, TorpedoModule> TorpedoModules { get; set; } = default!;
        public Dictionary<string, Engine> Engines { get; set; } = default!;
        public Dictionary<string, Hull> Hulls { get; set; } = default!;
        public Dictionary<string, PlaneData> CvPlanes { get; set; } = default!;
        public Dictionary<string, AirStrike> AirStrikes { get; set; } = default!;

        //may not need to be a List, but possibly an upgradeable module
        public Dictionary<string, PingerGun> PingerGunList { get; set; } = default!;
        public List<ShipConsumable> ShipConsumable { get; set; } = default!;
        public UpgradeInfo ShipUpgradeInfo { get; set; } = default!;
        public SpecialAbility? SpecialAbility { get; set; }
        public BurstModeAbility? BurstModeAbility { get; set; }
    }

    #region Fire Control

    public sealed record FireControl(decimal MaxRangeModifier, decimal SigmaModifier);

    #endregion

    #region Torpedo

    public sealed record TorpedoModule(decimal TimeToChangeAmmo, List<TorpedoLauncher> TorpedoLaunchers);

    public sealed record TorpedoLauncher(
        List<string> AmmoList,
        decimal BarrelDiameter,
        long Id,
        string Index,
        string Name,
        int NumBarrels,
        decimal HorizontalRotationSpeed,
        decimal VerticalRotationSpeed,
        decimal Reload,
        decimal[] HorizontalSector,
        decimal[][] HorizontalDeadZone,
        decimal[] TorpedoAngles);

    #endregion

    #region CvPlanes

    public sealed record PlaneData(PlaneType PlaneType, string PlaneName);

    #endregion

    #region AirStrike

    public sealed record AirStrike(
        int Charges,
        decimal FlyAwayTime,
        int MaximumDistance,
        int MaximumFlightDistance,
        int MinimumDistance,
        string PlaneName,
        decimal ReloadTime,
        decimal TimeBetweenShots,
        decimal DropTime);

    #endregion

    #region AA

    public class AntiAir
    {
        public AntiAirAura? LongRangeAura { get; set; }
        public AntiAirAura? MediumRangeAura { get; set; }
        public AntiAirAura? ShortRangeAura { get; set; }
    }

    public sealed record AntiAirAura(decimal HitChance, decimal MaxRange, decimal MinRange)
    {
        public const decimal DamageInterval = 0.285714285714m;

        public decimal ConstantDps { get; set; }
        public decimal FlakDamage { get; set; }
        public int FlakCloudsNumber { get; set; }

        public static AntiAirAura operator +(AntiAirAura thisAura, AntiAirAura newAura)
        {
            if (thisAura.MaxRange > 0 && (thisAura.MaxRange != newAura.MaxRange || thisAura.HitChance != newAura.HitChance))
            {
                throw new InvalidOperationException("Cannot combine auras with different ranges or accuracies.");
            }

            decimal minRange = newAura.FlakDamage > 0 ? thisAura.MinRange : newAura.MinRange; // Use minimum range of new aura only if it is no flak cloud aura
            
            return new(newAura.HitChance, newAura.MaxRange, minRange)
            {
                ConstantDps = thisAura.ConstantDps + newAura.ConstantDps,
                FlakDamage = thisAura.FlakDamage + newAura.FlakDamage,
                FlakCloudsNumber = thisAura.FlakCloudsNumber + newAura.FlakCloudsNumber,
            };
        }
    }

    #nullable restore
    #endregion

    #region Depth Charges

    public class DepthChargeArray
    {
        public int MaxPacks { get; set; }
        public decimal Reload { get; set; }
        public List<DepthChargeLauncher> DepthCharges { get; set; }
    }

    public class DepthChargeLauncher
    {
        public List<string> AmmoList { get; set; }
        public decimal[] HorizontalSector { get; set; }
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public int DepthChargesNumber { get; set; }
        public decimal[] RotationSpeed { get; set; }
    }

    #endregion

    #region Engine

    public class Engine
    {
        public decimal BackwardEngineUpTime { get; set; }
        public decimal ForwardEngineUpTime { get; set; }
        public decimal SpeedCoef { get; set; }
        public decimal ArmorCoeff { get; set; }
    }

    #endregion

    #region Hull

    public class Hull
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
        public AntiAir AntiAir { get; set; }
        public TurretModule SecondaryModule { get; set; }
        public DepthChargeArray DepthChargeArray { get; set; }
        public int FireSpots { get; set; }
        public decimal FireResistance { get; set; }
        public decimal FireTickDamage { get; set; }
        public decimal FireDuration { get; set; }
        public int FloodingSpots { get; set; }
        public decimal FloodingResistance { get; set; }
        public decimal FloodingTickDamage { get; set; }
        public decimal FloodingDuration { get; set; }
        public int TurningRadius { get; set; }
        public ShipSize Sizes { get; set; }
    }

    public class ShipSize
    {
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
    }

    #endregion

    #region PingerGun

    //copy of WG value. No idea of what we need or what is useful
    public class PingerGun
    {
        public decimal[] RotationSpeed { get; set; }
        public SectorParam[] SectorParams { get; set; }
        public decimal WaveDistance { get; set; }
        public int WaveHitAlertTime { get; set; }
        public int WaveHitLifeTime { get; set; }
        public WaveParam[] WaveParams { get; set; }
        public decimal WaveReloadTime { get; set; }
    }

    public class SectorParam
    {
        public decimal AlertTime { get; set; }
        public decimal Lifetime { get; set; }
        public decimal Width { get; set; }
        public decimal[][] WidthParams { get; set; }
    }

    public class WaveParam
    {
        public decimal EndWaveWidth { get; set; }
        public decimal EnergyCost { get; set; }
        public decimal StartWaveWidth { get; set; }
        public decimal[] WaveSpeed { get; set; }
    }

    #endregion

    #region Ship Consumables

    public class ShipConsumable
    {
        public int Slot { get; set; }
        public string ConsumableName { get; set; }
        public string ConsumableVariantName { get; set; }
    }

    #endregion

    #region Ship Upgrades and Modules

    public class UpgradeInfo
    {
        public List<ShipUpgrade> ShipUpgrades { get; set; }
        public int CostCredits { get; set; }
        public int CostGold { get; set; }
        public int CostSaleGold { get; set; }
        public int CostXp { get; set; }
        public int Value { get; set; }

        /// <summary>
        /// Helper method to easily filter all upgrade configurations of a specific type.
        /// </summary>
        /// <param name="componentType"></param>
        /// <returns>A list of all ship upgrades with the specified type.</returns>
        public List<ShipUpgrade> FindUpgradesOfType(ComponentType componentType)
        {
            return ShipUpgrades.Where(upgrade => upgrade.UcType == componentType).ToList();
        }

        /// <summary>
        /// Helper method to group all available ship upgrades by their type.
        /// </summary>
        /// <returns>A dictionary mapping the <see cref="ComponentType"/> of a <see cref="ShipUpgrade"/> to all upgrades available with that type.
        /// Stock upgrades appear first.</returns>
        public Dictionary<ComponentType, List<ShipUpgrade>> GroupUpgradesByType()
        {
            return ShipUpgrades.GroupBy(upgrade => upgrade.UcType)
                .ToDictionary(group => group.Key, group => group.OrderByDescending(upgrade => string.IsNullOrEmpty(upgrade.Prev)).ToList());
        }
    }

    //pretty much a copy of WG structure
    public class ShipUpgrade
    {
        public Dictionary<ComponentType, string[]> Components { get; set; }
        public string Name { get; set; }
        public string[] NextShips { get; set; }
        public string Prev { get; set; }
        public ComponentType UcType { get; set; }
        public bool CanBuy { get; set; }
    }

    #endregion

    #region Special Abilities for super ship
    public class SpecialAbility
    {
        public double Duration { get; set; }
        public int RequiredHits {get; set;}
        public double RadiusForSuccessfulHits { get; set; }
        public Dictionary<string, float> Modifiers { get; set; }
        public string Name { get; set; }
    }

    public class BurstModeAbility
    {
        public decimal ReloadDuringBurst { get; set; }
        public decimal ReloadAfterBurst { get; set; }
        public Dictionary<string, float> Modifiers { get; set; }
        public int ShotInBurst { get; set; }
    }
    #endregion
}