namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgDepthChargesArray : WgArmamentModule
{
    public Dictionary<string, WgDepthChargeLauncher> DepthCharges { get; init; } = new();

    public int MaxPacks { get; init; }

    public int NumShots { get; init; }

    public decimal ReloadTime { get; init; }
}

public class WgDepthChargeLauncher
{
    public string[] AmmoList { get; init; } = Array.Empty<string>();

    public decimal[] HorizSector { get; init; } = Array.Empty<decimal>();

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int NumBombs { get; init; }

    public decimal[] RotationSpeed { get; init; } = Array.Empty<decimal>();

    public TypeInfo TypeInfo { get; init; } = new();
}
