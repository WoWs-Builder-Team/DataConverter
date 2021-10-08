using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WoWsShipBuilderDataStructures
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
        [EnumMember(Value = "Artillery")]
        Artillery,

        [EnumMember(Value = "Hull")]
        Hull,

        [EnumMember(Value = "Sonar")]
        Sonar,

        [EnumMember(Value = "Torpedoes")]
        Torpedoes,

        [EnumMember(Value = "Fighter")]
        Fighter,

        [EnumMember(Value = "TorpedoBomber")]
        TorpedoBomber,

        [EnumMember(Value = "DiveBomber")]
        DiveBomber,

        [EnumMember(Value = "SkipBomber")]
        SkipBomber,

        [EnumMember(Value = "Suo")]
        Suo,

        [EnumMember(Value = "Engine")]
        Engine,

        [EnumMember(Value = "FlightControl")]
        FlightControl,

        [EnumMember(Value = "Secondary")]
        Secondary,

        [EnumMember(Value = "AirDefense")]
        AirDefense,

        [EnumMember(Value = "AirArmament")]
        AirArmament,

        [EnumMember(Value = "DepthCharges")]
        DepthCharges,

        [EnumMember(Value = "None")]
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
}