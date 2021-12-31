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

    #endregion

    #region Depth Charges

    public sealed record DepthChargeArray(int MaxPacks, decimal Reload, List<DepthChargeLauncher> DepthCharges);

    public sealed record DepthChargeLauncher(
        List<string> AmmoList,
        decimal[] HorizontalSector,
        long Id,
        string Index,
        string Name,
        int DepthChargesNumber,
        decimal[] RotationSpeed);

    #endregion

    #region Engine

    public sealed record Engine(decimal BackwardEngineUpTime, decimal ForwardEngineUpTime, decimal SpeedCoef, decimal ArmorCoeff);

    #endregion

    #region Hull

    public sealed record Hull(
        decimal Health,
        decimal MaxSpeed,
        decimal RudderTime,
        decimal SpeedCoef,
        decimal SteeringGearArmorCoeff,
        decimal SmokeFiringDetection,
        decimal SurfaceDetection,
        decimal AirDetection,
        decimal DetectionBySubPeriscope,
        decimal DetectionBySubOperating,
        int FireSpots,
        decimal FireResistance,
        decimal FireTickDamage,
        decimal FireDuration,
        int FloodingSpots,
        decimal FloodingResistance,
        decimal FloodingTickDamage,
        decimal FloodingDuration,
        int TurningRadius,
        ShipSize Sizes)
    {
        public AntiAir AntiAir { get; set; } = new();
        public TurretModule? SecondaryModule { get; set; }
        public DepthChargeArray? DepthChargeArray { get; set; }
    }

    public sealed record ShipSize(decimal Length, decimal Width, decimal Height);

    #endregion

    #region PingerGun

    //copy of WG value. No idea of what we need or what is useful
    public sealed record PingerGun(
        decimal[] RotationSpeed,
        SectorParam[] SectorParams,
        decimal WaveDistance,
        int WaveHitAlertTime,
        int WaveHitLifeTime,
        WaveParam[] WaveParams,
        decimal WaveReloadTime);

    public sealed record SectorParam(decimal AlertTime, decimal Lifetime, decimal Width, decimal[][] WidthParams);

    public sealed record WaveParam(decimal EndWaveWidth, decimal EnergyCost, decimal StartWaveWidth, decimal[] WaveSpeed);

    #endregion

    #region Ship Consumables

    public sealed record ShipConsumable(int Slot, string ConsumableName, string ConsumableVariantName);

    #endregion

    #region Ship Upgrades and Modules

    public sealed record UpgradeInfo(List<ShipUpgrade> ShipUpgrades, int CostCredits, int CostGold, int CostSaleGold, int CostXp, int Value)
    {
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
            return ShipUpgrades
                .GroupBy(upgrade => upgrade.UcType)
                .ToDictionary(
                    group => group.Key, 
                    group => group.OrderByDescending(upgrade => string.IsNullOrEmpty(upgrade.Prev)).ToList());
        }
    }

    //pretty much a copy of WG structure
    public sealed record ShipUpgrade(
        Dictionary<ComponentType, string[]> Components,
        string Name,
        string[] NextShips,
        string Prev,
        ComponentType UcType,
        bool CanBuy);

    #endregion

    #region Special Abilities for super ship

    public sealed record SpecialAbility(double Duration, int RequiredHits, double RadiusForSuccessfulHits, Dictionary<string, float> Modifiers, string Name);

    public sealed record BurstModeAbility(decimal ReloadDuringBurst, decimal ReloadAfterBurst, Dictionary<string, float> Modifiers, int ShotInBurst);

    #endregion
}
#nullable restore
