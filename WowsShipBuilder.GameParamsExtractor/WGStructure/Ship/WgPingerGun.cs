using WoWsShipBuilder.DataStructures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgPingerGun : WgArmamentModule
{
    public decimal[] RotationSpeed { get; set; } = Array.Empty<decimal>();

    public WgSectorParam[] SectorParams { get; set; } = Array.Empty<WgSectorParam>();

    public decimal WaveDistance { get; set; }

    public int WaveHitAlertTime { get; set; }

    public int WaveHitLifeTime { get; set; }

    public WgWaveParam[] WaveParams { get; set; } = Array.Empty<WgWaveParam>();

    public decimal WaveReloadTime { get; set; }

    public static implicit operator PingerGun(WgPingerGun thisWgPingerGun)
    {
        return new PingerGun
        {
            RotationSpeed = thisWgPingerGun.RotationSpeed,
            SectorParams = thisWgPingerGun.SectorParams.Select(wgSectorParam => (SectorParam)wgSectorParam).ToArray(),
            WaveDistance = thisWgPingerGun.WaveDistance,
            WaveHitAlertTime = thisWgPingerGun.WaveHitAlertTime,
            WaveHitLifeTime = thisWgPingerGun.WaveHitLifeTime,
            WaveParams = thisWgPingerGun.WaveParams.Select(wgWaveParam => (WaveParam)wgWaveParam).ToArray(),
            WaveReloadTime = thisWgPingerGun.WaveReloadTime,
        };
    }
}

public class WgSectorParam
{
    public decimal AlertTime { get; set; }

    public decimal Lifetime { get; set; }

    public decimal Width { get; set; }

    public decimal[][] WidthParams { get; set; } = Array.Empty<decimal[]>();

    public static implicit operator SectorParam(WgSectorParam thisSectorParam)
    {
        return new SectorParam
        {
            AlertTime = thisSectorParam.AlertTime,
            Lifetime = thisSectorParam.Lifetime,
            Width = thisSectorParam.Width,
            WidthParams = thisSectorParam.WidthParams,
        };
    }
}

public class WgWaveParam
{
    public decimal EndWaveWidth { get; set; }

    public decimal EnergyCost { get; set; }

    public decimal StartWaveWidth { get; set; }

    public decimal[] WaveSpeed { get; set; } = Array.Empty<decimal>();

    public static implicit operator WaveParam(WgWaveParam thisWaveParam)
    {
        return new WaveParam
        {
            EndWaveWidth = thisWaveParam.EndWaveWidth,
            EnergyCost = thisWaveParam.EnergyCost,
            StartWaveWidth = thisWaveParam.StartWaveWidth,
            WaveSpeed = thisWaveParam.WaveSpeed,
        };
    }
}
