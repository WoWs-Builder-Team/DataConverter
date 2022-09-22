namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgTorpedoArray : WgArmamentModule
{
    public decimal TimeToChangeAmmo { get; init; }

    public Dictionary<string, WgTorpedoLauncher> TorpedoArray { get; init; } = new();
}

public class WgTorpedoLauncher
{
    public string[] AmmoList { get; init; } = Array.Empty<string>();

    public decimal BarrelDiameter { get; init; }

    public decimal[][] DeadZone { get; init; } = Array.Empty<decimal[]>();

    public decimal[] HorizSector { get; init; } = Array.Empty<decimal>();

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int NumBarrels { get; init; }

    public decimal[] Position { get; init; } = Array.Empty<decimal>();

    public decimal[] RotationSpeed { get; init; } = Array.Empty<decimal>();

    public decimal ShotDelay { get; init; }

    public decimal[] TorpedoAngles { get; init; } = Array.Empty<decimal>(); //unknonw meaning, needed?

    public TypeInfo TypeInfo { get; init; } = new();
}
