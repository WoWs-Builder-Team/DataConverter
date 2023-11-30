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
    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public ProjectileType ProjectileType { get; init; }
}
