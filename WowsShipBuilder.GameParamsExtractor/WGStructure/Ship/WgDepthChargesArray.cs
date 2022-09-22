// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

using GameParamsExtractor.WGStructure;

namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgDepthChargesArray : WgArmamentModule
{
    public Dictionary<string, WgDepthChargeLauncher> DepthCharges { get; set; } = new();

    public int MaxPacks { get; set; }

    public int NumShots { get; set; }

    public decimal ReloadTime { get; set; }
}

public class WgDepthChargeLauncher
{
    public string[] AmmoList { get; set; } = Array.Empty<string>();

    public decimal[] HorizSector { get; set; } = Array.Empty<decimal>();

    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int NumBombs { get; set; }

    public decimal[] RotationSpeed { get; set; } = Array.Empty<decimal>();

    public TypeInfo TypeInfo { get; set; } = new();
}
