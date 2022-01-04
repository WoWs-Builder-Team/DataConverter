using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WoWsShipBuilder.DataStructures.Common
{
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
        Auxiliary
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
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlaneType
    {
        Fighter,
        DiveBomber,
        TorpedoBomber,
        SkipBomber,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlaneCategory
    {
        Cv,
        Consumable,
        Airstrike,
        Asw
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
        AP
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TorpedoType
    {
        Normal,
        DeepWater,
        Homing
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
        AP
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
}