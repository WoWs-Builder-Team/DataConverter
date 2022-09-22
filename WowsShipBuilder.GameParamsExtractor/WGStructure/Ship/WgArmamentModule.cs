using JsonSubTypes;
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

[JsonConverter(typeof(JsonSubtypes))]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgMainBattery), "guns")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgFireControl), "maxDistCoef")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgTorpedoArray), "torpedoArray")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgAtba), "antiAirAndSecondaries")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgAirSupport), "maxPlaneFlightDist")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgAirDefense), "isAA")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgDepthChargesArray), "depthCharges")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgEngine), "forwardEngineUpTime")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgHull), "visibilityFactor")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgFlightControl), "squadrons")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgPingerGun), "waveReloadTime")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgPlane), "planeType")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgPlane), "planes")]
[JsonSubtypes.KnownSubTypeWithProperty(typeof(WgSpecialAbility), "RageMode")]
public abstract class WgArmamentModule
{
}
