namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgFlightControl : WgArmamentModule
{
    public string[] Squadrons { get; init; } = Array.Empty<string>();
}

public class WgPlane : WgArmamentModule
{
    public string? PlaneType { get; init; }

    public string[]? Planes { get; init; }
}
