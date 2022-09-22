using JsonSubTypes;
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;

[JsonConverter(typeof(JsonSubtypes), "typeinfo.species")]
[JsonSubtypes.KnownSubType(typeof(WgShell), "Artillery")]
[JsonSubtypes.KnownSubType(typeof(WgBomb), "Bomb")]
[JsonSubtypes.KnownSubType(typeof(WgBomb), "SkipBomb")]
[JsonSubtypes.KnownSubType(typeof(WgTorpedo), "Torpedo")]
[JsonSubtypes.KnownSubType(typeof(WgDepthCharge), "DepthCharge")]
[JsonSubtypes.KnownSubType(typeof(WgRocket), "Rocket")]
public class WgProjectile : WgObject
{
    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
