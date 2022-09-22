using JsonSubTypes;
using Newtonsoft.Json;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

[JsonConverter(typeof(JsonSubtypes), "typeinfo.type")]
[JsonSubtypes.KnownSubType(typeof(WgConsumable), "Ability")]
[JsonSubtypes.KnownSubType(typeof(WgAircraft), "Aircraft")]
[JsonSubtypes.KnownSubType(typeof(WgCaptain), "Crew")]
[JsonSubtypes.KnownSubType(typeof(WgExterior), "Exterior")]
[JsonSubtypes.KnownSubType(typeof(WgModernization), "Modernization")]
[JsonSubtypes.KnownSubType(typeof(WgProjectile), "Projectile")]
[JsonSubtypes.KnownSubType(typeof(WgShip), "Ship")]
[JsonSubtypes.KnownSubType(typeof(WgModule), "Unit")]
public class WgObject
{
    public TypeInfo TypeInfo { get; init; } = new();
}

public class TypeInfo
{
    public string Nation { get; init; } = string.Empty;

    public string Species { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;
}

public class ModifiersPerClass
{
    public float AirCarrier { get; init; }

    public float Auxiliary { get; init; }

    public float Battleship { get; init; }

    public float Cruiser { get; init; }

    public float Destroyer { get; init; }

    public float Submarine { get; init; }
}
