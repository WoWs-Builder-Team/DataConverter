namespace WoWsShipBuilderDataStructures
{
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
        SecondaryWeaponsUnit,
        PrimaryWeaponsUnit,
    }

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
        Airstrike,
        Asw
    }

    public enum ProjectileType
    {
        Artillery,
        Bomb,
        SkipBomb,
        Torpedo,
        DepthCharge,
        Rocket,
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

    public enum ExteriorType
    {
        Flags,
        Camouflage,
        Permoflage,
    }

    public enum AntiAirAuraType
    {
        Near,
        Medium,
        Far,
    }

    public enum ShipCategory
    {
        TechTree,
        Premium,
        Special,
        TestShip,
        Disabled,
        Clan,
    }
}
