using System.Text.Json.Serialization;

namespace WoWsShipBuilderDataStructures
{
    public enum Nation
    {
        France,
        [JsonPropertyName("USA")]
        Usa,
        Russia,
        Japan,
        [JsonPropertyName("United_Kingdom")]
        UnitedKingdom,
        Germany,
        Italy,
        Europe,
        Netherlands,
        Spain,
        [JsonPropertyName("Pan_Asia")]
        PanAsia,
        [JsonPropertyName("Pan_America")]
        PanAmerica,
        Commonwealth
    }

    public enum ShipClass
    {
        Submarine,
        Destroyer,
        Cruiser,
        Battleship,
        AirCarrier,
        Auxiliary
    }

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
    }

    public enum ComponentType
    {
        DiveBomber,
        Fighter,
        Hull,
        Torpedoes,
        Suo,
        Engine,
        Artillery,
        FlightControl,
        Sonar,
        TorpedoBomber,
        SkipBomber,
        Secondary,
        AirDefense,
        AirArmament,
        DepthCharges,
        None,
    }

    public enum ModuleTier
    {
        Stock,
        UpgradeLevel1,
        UpgradeLevel2,
        None
    }

    public enum PlaneType
    {
        Fighter,
        DiveBomber,
        TorpedoBomber,
        SkipBomber,
    }

    public enum PlaneCategory
    {
        Cv,
        Consumable,
        Airstrike
    }

    public enum ProjectileType
    {
        Artillery,
        Bomb,
        SkipBomb,
        Torpedo,
        DepthCharge,
        Rocket        
    }
    public enum ShellType
    {
        SAP,
        HE,
        AP
    }
    public enum TorpedoType
    {
        Normal,
        DeepWater,
        Homing

    }
    public enum BombType
    {
        HE,
        AP,
    }
    public enum RocketType
    {
        HE,
        AP
    }

    public enum AntiAirAuraType
    {
        Near,
        Medium,
        Far,
    }
}
