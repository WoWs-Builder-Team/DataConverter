namespace WoWsShipBuilderDataStructures
{
    public enum Nation
    {
        France,
        USA,
        Russia,
        Japan,
        United_Kingdom,
        Germany,
        Italy,
        Europe,
        Netherlands,
        Spain,
        Pan_Asia,
        Pan_America,
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

}
