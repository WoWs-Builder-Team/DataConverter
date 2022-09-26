using JsonSubTypes;
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Projectile;

[JsonConverter(typeof(JsonSubtypes), "ProjectileType")]
[JsonSubtypes.KnownSubType(typeof(ArtilleryShell), ProjectileType.Artillery)]
[JsonSubtypes.KnownSubType(typeof(Bomb), ProjectileType.Bomb)]
[JsonSubtypes.KnownSubType(typeof(Bomb), ProjectileType.SkipBomb)]
[JsonSubtypes.KnownSubType(typeof(Rocket), ProjectileType.Rocket)]
[JsonSubtypes.KnownSubType(typeof(DepthCharge), ProjectileType.DepthCharge)]
[JsonSubtypes.KnownSubType(typeof(Torpedo), ProjectileType.Torpedo)]
public class Projectile
{
    public long Id { get; set; }
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ProjectileType ProjectileType { get; set; }
}
