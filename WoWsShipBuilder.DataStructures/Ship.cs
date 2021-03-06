using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.DataStructures.Components;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace WoWsShipBuilder.DataStructures;

public class Ship
{
    public long Id { get; set; }
    public string Index { get; set; }
    public string Name { get; set; }
    public int Tier { get; set; }
    public ShipClass ShipClass { get; set; }
    public ShipCategory ShipCategory { get; set; }
    public Nation ShipNation { get; set; }
    public List<string> Permoflages { get; set; }

    public Dictionary<string, TurretModule> MainBatteryModuleList { get; set; }
    public Dictionary<string, FireControl> FireControlList { get; set; }
    public Dictionary<string, TorpedoModule> TorpedoModules { get; set; }
    public Dictionary<string, Engine> Engines { get; set; }
    public Dictionary<string, Hull> Hulls { get; set; }
    public Dictionary<string, List<PlaneData>> CvPlanes { get; set; }
    public Dictionary<string, AirStrike> AirStrikes { get; set; }

    //may not need to be a List, but possibly an upgradeable module
    public Dictionary<string, PingerGun> PingerGunList { get; set; }
    public List<ShipConsumable> ShipConsumable { get; set; }
    public UpgradeInfo ShipUpgradeInfo { get; set; }
    public SpecialAbility SpecialAbility { get; set; }
}

#region Main Battery and Secondaries

//small abuse, but we reuse this for secondaries.
public class TurretModule
{
    public decimal Sigma { get; set; }
    public decimal MaxRange { get; set; }
    public List<Gun> Guns { get; set; }
    public AntiAirAura AntiAir { get; set; }
    public Dispersion DispersionValues { get; set; }
    public BurstModeAbility BurstModeAbility { get; set; }
}

public class Gun : IGun
{
    public List<string> AmmoList { get; set; }
    public decimal BarrelDiameter { get; set; }
    public decimal[] HorizontalSector { get; set; }
    public decimal[][] HorizontalDeadZones { get; set; }
    public long Id { get; set; }
    public string Index { get; set; }
    public string Name { get; set; }
    public int NumBarrels { get; set; }
    public decimal HorizontalPosition { get; set; }
    public decimal VerticalPosition { get; set; }
    public decimal HorizontalRotationSpeed { get; set; }
    public decimal VerticalRotationSpeed { get; set; }
    public decimal Reload { get; set; }
    public decimal SmokeDetectionWhenFiring { get; set; }
    public string WgGunIndex { get; set; }
    public decimal BaseAngle { get; set; }
}

public class Dispersion
{
    public double IdealRadius { get; set; }
    public double MinRadius { get; set; }
    public double IdealDistance { get; set; }
    public double TaperDist { get; set; }
    public double RadiusOnZero { get; set; }
    public double RadiusOnDelim { get; set; }
    public double RadiusOnMax { get; set; }
    public double Delim { get; set; }
    public decimal MaximumHorizontalDispersion { get; set; }
    public decimal MaximumVerticalDispersion { get; set; }

    public double CalculateHorizontalDispersion(double range)
    {
        // Calculation according to https://www.reddit.com/r/WorldOfWarships/comments/l1dpzt/reverse_engineered_dispersion_ellipse_including/
        double x = range / 30;
        double effectiveTaperDist = TaperDist / 30;
        if (x <= effectiveTaperDist)
        {
            return (x * (IdealRadius - MinRadius) / IdealDistance + MinRadius * (x / effectiveTaperDist)) * 30;
        }

        return (x * (IdealRadius - MinRadius) / IdealDistance + MinRadius) * 30;
    }

    public double CalculateVerticalDispersion(double maxRange, double range = -1)
    {
        // Calculation according to https://www.reddit.com/r/WorldOfWarships/comments/l1dpzt/reverse_engineered_dispersion_ellipse_including/
        maxRange /= 30;
        double x = range >= 0 ? range / 30 : maxRange;
        double delimDist = maxRange * Delim;

        double verticalCoeff;
        if (x < delimDist)
        {
            verticalCoeff = RadiusOnZero + (RadiusOnDelim - RadiusOnZero) * (x / delimDist);
        }
        else
        {
            verticalCoeff = RadiusOnDelim + (RadiusOnMax - RadiusOnDelim) * (x - delimDist) / (maxRange - delimDist);
        }

        return CalculateHorizontalDispersion(x * 30) * verticalCoeff;
    }
}
#endregion

#region Fire Control

public class FireControl
{
    public decimal MaxRangeModifier { get; set; }
    public decimal SigmaModifier { get; set; }
}

#endregion

#region Torpedo

public class TorpedoModule
{
    public decimal TimeToChangeAmmo { get; set; }
    public List<TorpedoLauncher> TorpedoLaunchers { get; set; }
}

public class TorpedoLauncher : IGun
{
    public List<string> AmmoList { get; set; }
    public decimal BarrelDiameter { get; set; }
    public long Id { get; set; }
    public string Index { get; set; }
    public string Name { get; set; }
    public decimal[][] HorizontalDeadZones { get; set; }
    public int NumBarrels { get; set; }
    public decimal HorizontalPosition { get; set; }
    public decimal VerticalPosition { get; set; }
    public decimal HorizontalRotationSpeed { get; set; }
    public decimal VerticalRotationSpeed { get; set; }
    public decimal Reload { get; set; }
    public string WgGunIndex { get; set; }
    public decimal[] HorizontalSector { get; set; }
    public decimal[] TorpedoAngles { get; set; }
    public decimal BaseAngle { get; set; }
}

#endregion

#region CvPlanes

public class PlaneData
{
    public PlaneType PlaneType { get; set; }
    public string PlaneName { get; set; }
}

#endregion

#region AirStrike

public class AirStrike
{
    public int Charges { get; set; }
    public decimal FlyAwayTime { get; set; }
    public int MaximumDistance { get; set; }
    public int MaximumFlightDistance { get; set; }
    public int MinimumDistance { get; set; }
    public string PlaneName { get; set; }
    public decimal ReloadTime { get; set; }
    public decimal TimeBetweenShots { get; set; }
    public decimal DropTime { get; set; }
}

#endregion

#region AA

public class AntiAir
{
    public AntiAirAura LongRangeAura { get; set; }
    public AntiAirAura MediumRangeAura { get; set; }
    public AntiAirAura ShortRangeAura { get; set; }
}

public class AntiAirAura
{
    public decimal ConstantDps { get; set; }
    public const decimal DamageInterval = 0.285714285714m;
    public decimal FlakDamage { get; set; }
    public int FlakCloudsNumber { get; set; }
    public decimal HitChance { get; set; }
    public decimal MaxRange { get; set; }
    public decimal MinRange { get; set; }

    public static AntiAirAura operator +(AntiAirAura thisAura, AntiAirAura newAura)
    {
        if (thisAura.MaxRange > 0 && (thisAura.MaxRange != newAura.MaxRange || thisAura.HitChance != newAura.HitChance))
        {
            throw new InvalidOperationException("Cannot combine auras with different ranges or accuracies.");
        }

        decimal minRange = newAura.FlakDamage > 0 ? thisAura.MinRange : newAura.MinRange; // Use minimum range of new aura only if it is no flak cloud aura

        return new AntiAirAura
        {
            ConstantDps = thisAura.ConstantDps + newAura.ConstantDps,
            FlakDamage = thisAura.FlakDamage + newAura.FlakDamage,
            FlakCloudsNumber = thisAura.FlakCloudsNumber + newAura.FlakCloudsNumber,
            HitChance = newAura.HitChance,
            MaxRange = newAura.MaxRange,
            MinRange = minRange,
        };
    }
}

#endregion

#region Depth Charges

public class DepthChargeArray
{
    public int MaxPacks { get; set; }
    public int NumShots { get; set; }
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
    public List<HitLocation> HitLocations { get; set; }
}

public class ShipSize
{
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
}

public class HitLocation
{
    public ShipHitLocation Name { get; set; }
    public string Type { get; set; } = string.Empty;
    public float RepairableDamage { get; set; }
    public int Hp { get; set; }
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
    /// <param name="componentType">The type of the component to look for.</param>
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
    public int RequiredHits { get; set; }
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
