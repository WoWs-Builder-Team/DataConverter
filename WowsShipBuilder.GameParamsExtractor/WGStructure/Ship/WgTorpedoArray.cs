// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgTorpedoArray : WgArmamentModule
{
    public decimal TimeToChangeAmmo { get; set; }

    public Dictionary<string, WgTorpedoLauncher> TorpedoArray { get; set; } = new();
}

public class WgTorpedoLauncher
{
    public string[] AmmoList { get; set; } = Array.Empty<string>();

    public decimal BarrelDiameter { get; set; }

    public decimal[][] DeadZone { get; set; } = Array.Empty<decimal[]>();

    public decimal[] HorizSector { get; set; } = Array.Empty<decimal>();

    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int NumBarrels { get; set; }

    public decimal[] Position { get; set; } = Array.Empty<decimal>();

    public decimal[] RotationSpeed { get; set; } = Array.Empty<decimal>();

    public decimal ShotDelay { get; set; }

    public decimal[] TorpedoAngles { get; set; } = Array.Empty<decimal>(); //unknonw meaning, needed?

    public TypeInfo TypeInfo { get; set; } = new();
}
