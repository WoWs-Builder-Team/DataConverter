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
        Bomber,
        Dive,
        Skip
    }

    public enum PlaneCategory
    {
        Cv,
        Consumable,
        Airstrike
    }

}
