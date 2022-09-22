// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgFlightControl : WgArmamentModule
{
    public string[] Squadrons { get; set; } = Array.Empty<string>();
}

public class WgPlane : WgArmamentModule
{
    public string? PlaneType { get; set; }

    public string[]? Planes { get; set; }
}
