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
}

public class WgSectorParam
{
    public decimal AlertTime { get; set; }

    public decimal Lifetime { get; set; }

    public decimal Width { get; set; }

    public decimal[][] WidthParams { get; set; } = Array.Empty<decimal[]>();
}

public class WgWaveParam
{
    public decimal EndWaveWidth { get; set; }

    public decimal EnergyCost { get; set; }

    public decimal StartWaveWidth { get; set; }

    public decimal[] WaveSpeed { get; set; } = Array.Empty<decimal>();
}
