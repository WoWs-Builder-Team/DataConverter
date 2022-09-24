namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgPingerGun : WgArmamentModule
{
    public decimal[] RotationSpeed { get; init; } = Array.Empty<decimal>();

    public WgSectorParam[] SectorParams { get; init; } = Array.Empty<WgSectorParam>();

    public decimal WaveDistance { get; init; }

    public int WaveHitAlertTime { get; init; }

    public int WaveHitLifeTime { get; init; }

    public WgWaveParam[] WaveParams { get; init; } = Array.Empty<WgWaveParam>();

    public decimal WaveReloadTime { get; init; }
}

public class WgSectorParam
{
    public decimal AlertTime { get; init; }

    public decimal Lifetime { get; init; }

    public decimal Width { get; init; }

    public decimal[][] WidthParams { get; init; } = Array.Empty<decimal[]>();
}

public class WgWaveParam
{
    public decimal EndWaveWidth { get; init; }

    public decimal EnergyCost { get; init; }

    public decimal StartWaveWidth { get; init; }

    public decimal[] WaveSpeed { get; init; } = Array.Empty<decimal>();
}
