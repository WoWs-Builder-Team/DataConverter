using System.Collections.Immutable;

namespace WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class PingerGun
{
    public ImmutableArray<decimal> RotationSpeed { get; init; } = ImmutableArray<decimal>.Empty;

    public ImmutableArray<SectorParam> SectorParams { get; init; } = ImmutableArray<SectorParam>.Empty;

    public decimal WaveDistance { get; init; }

    public int WaveHitAlertTime { get; init; }

    public int WaveHitLifeTime { get; init; }

    public ImmutableArray<WaveParam> WaveParams { get; init; } = ImmutableArray<WaveParam>.Empty;

    public decimal WaveReloadTime { get; init; }
}

public class SectorParam
{
    public decimal AlertTime { get; init; }

    public decimal Lifetime { get; init; }

    public decimal Width { get; init; }

    public ImmutableArray<ImmutableArray<decimal>> WidthParams { get; init; } = ImmutableArray<ImmutableArray<decimal>>.Empty;
}

public class WaveParam
{
    public decimal EndWaveWidth { get; init; }

    public decimal EnergyCost { get; init; }

    public decimal StartWaveWidth { get; init; }

    public ImmutableArray<decimal> WaveSpeed { get; init; } = ImmutableArray<decimal>.Empty;
}
