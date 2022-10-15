using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures;

public sealed record ShipSummary(string Index, Nation Nation, int Tier, ShipClass ShipClass, ShipCategory Category, string? PrevShipIndex, List<string>? NextShipsIndex = null);
