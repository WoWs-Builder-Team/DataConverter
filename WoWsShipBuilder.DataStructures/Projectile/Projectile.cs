using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Projectile;

[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(ArtilleryShell), "artillery")]
[JsonDerivedType(typeof(Bomb), "bomb")]
[JsonDerivedType(typeof(Rocket), "rocket")]
[JsonDerivedType(typeof(DepthCharge), "depth_charge")]
[JsonDerivedType(typeof(Torpedo), "torpedo")]
public class Projectile
{
    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ProjectileType ProjectileType { get; set; }
}
