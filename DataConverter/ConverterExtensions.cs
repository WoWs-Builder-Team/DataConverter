using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

namespace DataConverter;

public static class ConverterExtensions
{
    public static Gun ConvertData(this WgGun wgGun, double taperDist, string wgGunIndex, decimal gunBaseAngle) => new()
    {
        AmmoList = wgGun.AmmoList.ToImmutableArray(),
        BarrelDiameter = wgGun.BarrelDiameter,
        HorizontalSector = wgGun.HorizSector.ToImmutableArray(),
        HorizontalDeadZones = wgGun.DeadZone.Select(x => x.ToImmutableArray()).ToImmutableArray(),
        Id = wgGun.Id,
        Index = wgGun.Index,
        Name = wgGun.Name,
        NumBarrels = wgGun.NumBarrels,
        HorizontalPosition = wgGun.SmallGun ? 0 : wgGun.Position[1],
        VerticalPosition = wgGun.SmallGun ? 0 : wgGun.Position[0],
        HorizontalRotationSpeed = wgGun.RotationSpeed[0],
        VerticalRotationSpeed = wgGun.RotationSpeed[1],
        Reload = wgGun.ShotDelay,
        SmokeDetectionWhenFiring = wgGun.SmokePenalty,
        AmmoSwitchCoeff = wgGun.AmmoSwitchCoeff,
        Dispersion = new()
        {
            IdealRadius = wgGun.IdealRadius,
            MinRadius = wgGun.MinRadius,
            IdealDistance = wgGun.IdealDistance,
            TaperDist = taperDist,
            RadiusOnZero = wgGun.RadiusOnZero,
            RadiusOnDelim = wgGun.RadiusOnDelim,
            RadiusOnMax = wgGun.RadiusOnMax,
            Delim = wgGun.Delim,
        },
        WgGunIndex = wgGunIndex,
        BaseAngle = gunBaseAngle,
    };

    public static AntiAirAura ConvertData(this WgAaAura wgAura) => new()
    {
        ConstantDps = wgAura.AreaDamage,
        FlakDamage = wgAura.BubbleDamage,
        FlakCloudsNumber = wgAura.InnerBubbleCount + wgAura.OuterBubbleCount,
        HitChance = wgAura.HitChance,
        MaxRange = wgAura.MaxDistance,
        MinRange = wgAura.MinDistance,
    };

    /// <summary>
    /// Converts the Aimed Fire block of an air defense module.
    /// </summary>
    /// <param name="wgAimedFire">The raw Aimed Fire parameters.</param>
    /// <param name="shipClass">
    /// The class of the ship owning the module. The game data tunes the two damage multipliers per owning class, so
    /// they are resolved here into plain numbers instead of being carried around as a map or as a modifier.
    /// </param>
    /// <returns>The converted Aimed Fire parameters.</returns>
    public static AntiAirAimedFire ConvertData(this WgAimedFire wgAimedFire, ShipClass shipClass)
    {
        // The game states the charge gain as an increment applied on a repeating timer. Every module currently uses a
        // one second period, but guard the division rather than assume it.
        decimal tickPeriod = wgAimedFire.GameLogicTrigger?.Activator?.Duration ?? 0;
        decimal chargePerTick = wgAimedFire.GameLogicTrigger?.Action?.ProgressIncrement ?? 0;

        return new()
        {
            RequiredCharge = wgAimedFire.RequiredCharge,
            ChargeGainRate = tickPeriod > 0 ? chargePerTick / tickPeriod : 0,
            ChargeSpendingRate = wgAimedFire.ChargeSpendingRate,
            DecrementDelay = wgAimedFire.DecrementDelay,
            DecrementRate = wgAimedFire.DecrementRate,
            InstantDamageCooldown = wgAimedFire.InstantDamageCooldown,
            InstantDamagePercentage = wgAimedFire.InstantDamagePercentageByClass.ToImmutableDictionary(),

            // A class missing from the map - or a module stating no multipliers at all - means the mechanic does not
            // change that stat, which is a factor of one.
            AuraDamageMultiplier = wgAimedFire.Modifiers?.AuraDamageFor(shipClass) ?? 1m,
            BubbleDamageMultiplier = wgAimedFire.Modifiers?.BubbleDamageFor(shipClass) ?? 1m,
        };
    }

    public static AirStrike ConvertData(this WgAirSupport wgAirSupport) => new()
    {
        Charges = wgAirSupport.ChargesNum,
        FlyAwayTime = wgAirSupport.FlyAwayTime,
        MaximumDistance = wgAirSupport.MaxDist,
        MaximumFlightDistance = wgAirSupport.MaxPlaneFlightDist,
        MinimumDistance = wgAirSupport.MinDist,
        PlaneName = string.IsNullOrEmpty(wgAirSupport.PlaneName) ? wgAirSupport.AmmoList.FirstOrDefault(string.Empty) : wgAirSupport.PlaneName,
        DropTime = wgAirSupport.TimeFromHeaven,
        ReloadTime = wgAirSupport.ReloadTime,
        TimeBetweenShots = wgAirSupport.TimeBetweenShots,
    };

    public static DepthChargeLauncher ConvertData(this WgDepthChargeLauncher wgLauncher) => new()
    {
        AmmoList = wgLauncher.AmmoList.ToImmutableArray(),
        DepthChargesNumber = wgLauncher.NumBombs,
        HorizontalSector = wgLauncher.HorizSector.ToImmutableArray(),
        Id = wgLauncher.Id,
        Index = wgLauncher.Index,
        Name = wgLauncher.Name,
        RotationSpeed = wgLauncher.RotationSpeed.ToImmutableArray(),
    };

    public static PingerGun ConvertData(this WgPingerGun wgPingerGun) => new()
    {
        RotationSpeed = wgPingerGun.RotationSpeed.ToImmutableArray(),
        SectorParams = wgPingerGun.SectorParams.Select(wgSectorParam => wgSectorParam.ConvertData()).ToImmutableArray(),
        WaveDistance = wgPingerGun.WaveDistance,
        WaveHitAlertTime = wgPingerGun.WaveHitAlertTime,
        WaveHitLifeTime = wgPingerGun.WaveHitLifeTime,
        WaveParams = wgPingerGun.WaveParams.Select(wgWaveParam => wgWaveParam.ConvertData()).ToImmutableArray(),
        WaveReloadTime = wgPingerGun.WaveReloadTime,
    };

    public static SectorParam ConvertData(this WgSectorParam wgSectorParam) => new()
    {
        AlertTime = wgSectorParam.AlertTime,
        Lifetime = wgSectorParam.Lifetime,
        Width = wgSectorParam.Width,
        WidthParams = wgSectorParam.WidthParams.Select(x => x.ToImmutableArray()).ToImmutableArray(),
    };

    public static WaveParam ConvertData(this WgWaveParam wgWaveParam) => new()
    {
        EndWaveWidth = wgWaveParam.EndWaveWidth,
        EnergyCost = wgWaveParam.EnergyCost,
        StartWaveWidth = wgWaveParam.StartWaveWidth,
        WaveSpeed = wgWaveParam.WaveSpeed.ToImmutableArray(),
    };

    public static TorpedoLauncher ConvertData(this WgTorpedoLauncher wgTorpedoLauncher, string groupName, decimal baseAngle) => new()
    {
        AmmoList = wgTorpedoLauncher.AmmoList.ToImmutableArray(),
        BarrelDiameter = wgTorpedoLauncher.BarrelDiameter,
        Id = wgTorpedoLauncher.Id,
        Index = wgTorpedoLauncher.Index,
        Name = wgTorpedoLauncher.Name,
        HorizontalDeadZones = wgTorpedoLauncher.DeadZone.Select(x => x.ToImmutableArray()).ToImmutableArray(),
        NumBarrels = wgTorpedoLauncher.NumBarrels,
        HorizontalPosition = wgTorpedoLauncher.Position[1],
        VerticalPosition = wgTorpedoLauncher.Position[0],
        HorizontalRotationSpeed = wgTorpedoLauncher.RotationSpeed[0],
        VerticalRotationSpeed = wgTorpedoLauncher.RotationSpeed[1],
        Reload = wgTorpedoLauncher.ShotDelay,
        HorizontalSector = wgTorpedoLauncher.HorizSector.ToImmutableArray(),
        TorpedoAngles = wgTorpedoLauncher.TorpedoAngles.ToImmutableArray(),
        AmmoSwitchCoeff = wgTorpedoLauncher.AmmoSwitchCoeff,
        GroupName = groupName,
        BaseAngle = baseAngle,
    };
}
