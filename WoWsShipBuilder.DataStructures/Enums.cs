using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WoWsShipBuilder.DataStructures;

[JsonConverter(typeof(StringEnumConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
public enum ShipClass
{
    Submarine,
    Destroyer,
    Cruiser,
    Battleship,
    AirCarrier,
    Auxiliary,
}

[JsonConverter(typeof(StringEnumConverter))]
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
[JsonConverter(typeof(StringEnumConverter))]
public enum ComponentType
{
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
    None,
    TacticalFighter,
    TacticalDiveBomber,
    TacticalTorpedoBomber,
    TacticalSkipBomber,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PlaneType
{
    Fighter,
    DiveBomber,
    TorpedoBomber,
    SkipBomber,
    TacticalFighter,
    TacticalDiveBomber,
    TacticalTorpedoBomber,
    TacticalSkipBomber,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PlaneCategory
{
    Cv,
    Consumable,
    Airstrike,
    Asw,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ProjectileType
{
    Artillery,
    Bomb,
    SkipBomb,
    Torpedo,
    DepthCharge,
    Rocket,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ShellType
{
    SAP,
    HE,
    AP,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum TorpedoType
{
    Standard,
    DeepWater,
    Magnetic,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum BombType
{
    HE,
    AP,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum RocketType
{
    HE,
    AP,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ExteriorType
{
    Flags,
    Camouflage,
    Permoflage,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum AntiAirAuraType
{
    Near,
    Medium,
    Far,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ShipCategory
{
    TechTree,
    Premium,
    Special,
    TestShip,
    Disabled,
    Clan,
    SuperShip,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ModernizationType
{
    Normal,
    Consumable,
    Legendary,
    Other,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum TurretOrientation
{
    Forward,
    Backward,
    Right,
    Left,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum GameVersionType
{
    Live,
    Pts,
    Dev1,
    Dev2,
    Dev3,

    [Obsolete("Remains only for backwards compatibility with older version info files.", true)]
    Dev,
}

[JsonConverter(typeof(StringEnumConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
public enum SubsBuoyancyStates
{
    DeepWater,
    Periscope,
    Surface,
}
