using System.Linq;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

namespace DataConverter;

public static class ConverterExtensions
{
    public static Gun ConvertData(this WgGun wgGun) => new()
    {
        AmmoList = wgGun.AmmoList.ToList(),
        BarrelDiameter = wgGun.BarrelDiameter,
        HorizontalSector = wgGun.HorizSector,
        HorizontalDeadZones = wgGun.DeadZone,
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
        AmmoList = wgLauncher.AmmoList.ToList(),
        DepthChargesNumber = wgLauncher.NumBombs,
        HorizontalSector = wgLauncher.HorizSector,
        Id = wgLauncher.Id,
        Index = wgLauncher.Index,
        Name = wgLauncher.Name,
        RotationSpeed = wgLauncher.RotationSpeed,
    };

    public static PingerGun ConvertData(this WgPingerGun wgPingerGun) => new()
    {
        RotationSpeed = wgPingerGun.RotationSpeed,
        SectorParams = wgPingerGun.SectorParams.Select(wgSectorParam => wgSectorParam.ConvertData()).ToArray(),
        WaveDistance = wgPingerGun.WaveDistance,
        WaveHitAlertTime = wgPingerGun.WaveHitAlertTime,
        WaveHitLifeTime = wgPingerGun.WaveHitLifeTime,
        WaveParams = wgPingerGun.WaveParams.Select(wgWaveParam => wgWaveParam.ConvertData()).ToArray(),
        WaveReloadTime = wgPingerGun.WaveReloadTime,
    };

    public static SectorParam ConvertData(this WgSectorParam wgSectorParam) => new()
    {
        AlertTime = wgSectorParam.AlertTime,
        Lifetime = wgSectorParam.Lifetime,
        Width = wgSectorParam.Width,
        WidthParams = wgSectorParam.WidthParams,
    };

    public static WaveParam ConvertData(this WgWaveParam wgWaveParam) => new()
    {
        EndWaveWidth = wgWaveParam.EndWaveWidth,
        EnergyCost = wgWaveParam.EnergyCost,
        StartWaveWidth = wgWaveParam.StartWaveWidth,
        WaveSpeed = wgWaveParam.WaveSpeed,
    };

    public static TorpedoLauncher ConvertData(this WgTorpedoLauncher wgTorpedoLauncher) => new()
    {
        AmmoList = wgTorpedoLauncher.AmmoList.ToList(),
        BarrelDiameter = wgTorpedoLauncher.BarrelDiameter,
        Id = wgTorpedoLauncher.Id,
        Index = wgTorpedoLauncher.Index,
        Name = wgTorpedoLauncher.Name,
        HorizontalDeadZones = wgTorpedoLauncher.DeadZone,
        NumBarrels = wgTorpedoLauncher.NumBarrels,
        HorizontalPosition = wgTorpedoLauncher.Position[1],
        VerticalPosition = wgTorpedoLauncher.Position[0],
        HorizontalRotationSpeed = wgTorpedoLauncher.RotationSpeed[0],
        VerticalRotationSpeed = wgTorpedoLauncher.RotationSpeed[1],
        Reload = wgTorpedoLauncher.ShotDelay,
        HorizontalSector = wgTorpedoLauncher.HorizSector,
        TorpedoAngles = wgTorpedoLauncher.TorpedoAngles,
    };
}
