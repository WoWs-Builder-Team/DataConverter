using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace WoWsShipBuilder.DataStructures;

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Nation
{
    France,
    Usa,
    Russia,
    Japan,
    UnitedKingdom,
    Germany,
    Italy,
    Europe,
    Netherlands,
    Spain,
    PanAsia,
    PanAmerica,
    Commonwealth,
    Common,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShipClass
{
    Submarine,
    Destroyer,
    Cruiser,
    Battleship,
    AirCarrier,
    Auxiliary,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModuleType
{
    DiveBomberTypeUnit,
    FighterTypeUnit,
    HullUnit,
    TorpedoesUnit,
    SuoUnit,
    EngineUnit,
    ArtilleryUnit,
    FlightControlUnit,
    SonarUnit,
    TorpedoBomberTypeUnit,
    SkipBomberTypeUnit,
    SecondaryWeaponsUnit,
    PrimaryWeaponsUnit,
}

// When new types are added, make sure they are also added in ShipConverter.FindModuleType
[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComponentType
{
    None,
    Artillery,
    Hull,
    Sonar,
    Torpedoes,
    Fighter,
    TorpedoBomber,
    DiveBomber,
    SkipBomber,
    Suo,
    Engine,
    FlightControl,
    Secondary,
    AirDefense,
    AirArmament,
    DepthCharges,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlaneType
{
    None,
    Fighter,
    DiveBomber,
    TorpedoBomber,
    SkipBomber,
    TacticalFighter,
    TacticalDiveBomber,
    TacticalTorpedoBomber,
    TacticalSkipBomber,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlaneCategory
{
    Cv,
    Consumable,
    Airstrike,
    Asw,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProjectileType
{
    Artillery,
    Bomb,
    SkipBomb,
    Torpedo,
    DepthCharge,
    Rocket,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "WG uses this naming convention")]
public enum ShellType
{
    SAP,
    HE,
    AP,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TorpedoType
{
    Standard,
    DeepWater,
    Magnetic,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "WG uses this naming convention")]
public enum BombType
{
    HE,
    AP,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "WG uses this naming convention")]
public enum RocketType
{
    HE,
    AP,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExteriorType
{
    Flags,
    Camouflage,
    Permoflage,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AntiAirAuraType
{
    Near,
    Medium,
    Far,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShipCategory
{
    TechTree,
    Premium,
    Special,
    TestShip,
    Disabled,
    Clan,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModernizationType
{
    Normal,
    Consumable,
    Legendary,
    Other,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TurretOrientation
{
    Forward,
    Backward,
    Right,
    Left,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameVersionType
{
    Live,
    Pts,
    Dev1,
    Dev2,
    Dev3,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShipHitLocation
{
    Bow,
    Stern,
    AuxiliaryRooms,
    Casemate,
    Citadel,
    Superstructure,
    Hull,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubmarineBuoyancyStates
{
    DeepWater,
    Periscope,
    Surface,
}

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubmarineTorpedoLauncherLoaderPosition
{
    BowLoaders,
    SternLoaders,
}
