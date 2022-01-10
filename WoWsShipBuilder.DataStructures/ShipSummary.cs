#nullable enable
using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures
{
    public sealed record ShipSummary(string Index, Nation Nation, int Tier, ShipClass ShipClass, ShipCategory Category, string? PrevShipIndex, List<string>? NextShipsIndex = null);
}
#nullable restore