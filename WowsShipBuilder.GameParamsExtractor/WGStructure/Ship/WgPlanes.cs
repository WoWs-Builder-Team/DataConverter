namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgFlightControl : WgArmamentModule
{
    public string[] Squadrons { get; set; } = Array.Empty<string>();
}

public class WgPlane : WgArmamentModule
{
    public string? PlaneType { get; set; }

    public string[]? Planes { get; set; }
}
