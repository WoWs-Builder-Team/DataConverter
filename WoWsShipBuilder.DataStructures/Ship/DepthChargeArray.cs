using System;
using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class DepthChargeArray
{
    public int MaxPacks { get; init; }

    public int NumShots { get; init; }

    public decimal Reload { get; init; }

    public List<DepthChargeLauncher> DepthCharges { get; init; } = new();
}

public class DepthChargeLauncher
{
    public List<string> AmmoList { get; init; } = new();

    public decimal[] HorizontalSector { get; init; } = Array.Empty<decimal>();

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int DepthChargesNumber { get; init; }

    public decimal[] RotationSpeed { get; init; } = Array.Empty<decimal>();
}
