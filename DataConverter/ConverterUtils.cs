using System;
using WoWsShipBuilder.DataStructures;

namespace DataConverter;

public static class ConverterUtils
{
    public static ShipClass ProcessShipClass(string shipClass)
    {
        return shipClass.ToLowerInvariant() switch
        {
            "cruiser" => ShipClass.Cruiser,
            "destroyer" => ShipClass.Destroyer,
            "battleship" => ShipClass.Battleship,
            "aircarrier" => ShipClass.AirCarrier,
            "submarine" => ShipClass.Submarine,
            "auxiliary" => ShipClass.Auxiliary,
            _ => throw new InvalidOperationException("Ship class not recognized."),
        };
    }
}
