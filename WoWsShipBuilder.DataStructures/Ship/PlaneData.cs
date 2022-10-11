using System;

namespace WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
[Obsolete("No longer used, plane type is now located in Aircraft.cs", true)]
public class PlaneData
{
    public PlaneType PlaneType { get; init; }

    public string PlaneName { get; init; } = string.Empty;
}
