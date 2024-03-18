using System.Collections.Immutable;
using System.Linq;
using WoWsShipBuilder.DataStructures.Ship;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

namespace DataConverter;

public static class ConverterExtensions
{
    public static Gun ConvertData(this WgGun wgGun, double taperDist, string wgGunIndex, decimal gunBaseAngle, ImmutableArray<string> additionalAmmo) => new()
    {
        AmmoList = wgGun.AmmoList.Concat(additionalAmmo).ToImmutableArray(),
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

    public static AirStrike ConvertData(this WgAirSupport wgAirSupport) => new()
    {
        Charges = wgAirSupport.ChargesNum,
        FlyAwayTime = wgAirSupport.FlyAwayTime,
        MaximumDistance = wgAirSupport.MaxDist,
        MaximumFlightDistance = wgAirSupport.MaxPlaneFlightDist,
        MinimumDistance = wgAirSupport.MinDist,
        PlaneName = wgAirSupport.PlaneName,
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
