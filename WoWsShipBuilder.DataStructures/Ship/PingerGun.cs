using System;

namespace WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class PingerGun
{
    public decimal[] RotationSpeed { get; init; } = Array.Empty<decimal>();

    public SectorParam[] SectorParams { get; init; } = Array.Empty<SectorParam>();

    public decimal WaveDistance { get; init; }

    public int WaveHitAlertTime { get; init; }

    public int WaveHitLifeTime { get; init; }

    public WaveParam[] WaveParams { get; init; } = Array.Empty<WaveParam>();

    public decimal WaveReloadTime { get; init; }
}

public class SectorParam
{
    public decimal AlertTime { get; init; }

    public decimal Lifetime { get; init; }

    public decimal Width { get; init; }

    public decimal[][] WidthParams { get; init; } = Array.Empty<decimal[]>();
}

public class WaveParam
{
    public decimal EndWaveWidth { get; init; }

    public decimal EnergyCost { get; init; }

    public decimal StartWaveWidth { get; init; }

    public decimal[] WaveSpeed { get; init; } = Array.Empty<decimal>();
}
