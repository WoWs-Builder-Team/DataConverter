using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class DepthChargeArray
{
    public int MaxPacks { get; init; }

    public int NumShots { get; init; }

    public decimal Reload { get; init; }

    public ImmutableArray<DepthChargeLauncher> DepthCharges { get; init; } = ImmutableArray<DepthChargeLauncher>.Empty;
}

public class DepthChargeLauncher
{
    public ImmutableArray<string> AmmoList { get; init; } = ImmutableArray<string>.Empty;

    public ImmutableArray<decimal> HorizontalSector { get; init; } = ImmutableArray<decimal>.Empty;

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int DepthChargesNumber { get; init; }

    public ImmutableArray<decimal> RotationSpeed { get; init; } = ImmutableArray<decimal>.Empty;
}
