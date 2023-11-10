using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Global
namespace WoWsShipBuilder.DataStructures;

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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlaneCategory
{
    Cv,
    Consumable,
    Airstrike,
    Asw,
}

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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShellType
{
    SAP,
    HE,
    AP,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TorpedoType
{
    Standard,
    DeepWater,
    Magnetic,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BombType
{
    HE,
    AP,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RocketType
{
    HE,
    AP,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExteriorType
{
    Flags,
    Camouflage,
    Permoflage,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AntiAirAuraType
{
    Near,
    Medium,
    Far,
}

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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModernizationType
{
    Normal,
    Consumable,
    Legendary,
    Other,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TurretOrientation
{
    Forward,
    Backward,
    Right,
    Left,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameVersionType
{
    Live,
    Pts,
    Dev1,
    Dev2,
    Dev3,
}

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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubsBuoyancyStates
{
    DeepWater,
    Periscope,
    Surface,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubTorpLauncherLoaderPosition
{
    BowLoaders,
    SternLoaders,
}
