using GameParamsExtractor.WGStructure;
using JsonSubTypes;
using Newtonsoft.Json;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

[JsonConverter(typeof(JsonSubtypes), "TypeInfo.Type")]
[JsonSubtypes.KnownSubType(typeof(WgConsumable), "Ability")]
[JsonSubtypes.KnownSubType(typeof(WgAircraft), "Aircraft")]
[JsonSubtypes.KnownSubType(typeof(WgCaptain), "Crew")]
[JsonSubtypes.KnownSubType(typeof(WgExterior), "Exterior")]
[JsonSubtypes.KnownSubType(typeof(WGModernization), "Modernization")]
[JsonSubtypes.KnownSubType(typeof(WGProjectile), "Projectile")]
[JsonSubtypes.KnownSubType(typeof(WgShip), "Ship")]
[JsonSubtypes.KnownSubType(typeof(WGModule), "Unit")]
public abstract class WgObject
{
    public TypeInfo TypeInfo { get; set; } = new();
}

public class TypeInfo
{
    public string Nation { get; set; } = string.Empty;

    public string Species { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}

public class ModifiersPerClass
{
    public float AirCarrier { get; set; }

    public float Auxiliary { get; set; }

    public float Battleship { get; set; }

    public float Cruiser { get; set; }

    public float Destroyer { get; set; }

    public float Submarine { get; set; }
}
