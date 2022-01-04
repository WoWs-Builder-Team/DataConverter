using System.Collections.Generic;
using WoWsShipBuilder.DataStructures.Common;

// ReSharper disable NotAccessedPositionalProperty.Global

namespace WoWsShipBuilder.DataStructures.Ship
{
    public sealed record ShipSummary(string Index, Nation Nation, int Tier, ShipClass ShipClass, ShipCategory Category, string PrevShipIndex, List<string> NextShipsIndex);
}