using System;
using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures;

/// <summary>
/// Container class for turret position overrides.
/// </summary>
/// <param name="ArtilleryTurretOverrides">Maps the name of an artillery module to a dictionary mapping the turret keys to their position overrides.</param>
[Obsolete("No longer used", true)]
public sealed record ShipTurretOverride(Dictionary<string, Dictionary<string, TurretOrientation>> ArtilleryTurretOverrides);
