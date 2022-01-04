using System.Collections.Generic;
using WoWsShipBuilder.DataStructures.Common;

namespace WoWsShipBuilder.DataStructures.Ship
{
    /// <summary>
    /// A container class for ship turret direction overrides.
    /// </summary>
    /// <param name="ArtilleryTurretOverrides">Maps the name of an artillery module to a dictionary mapping the turret keys to their position overrides.</param>
    public sealed record ShipTurretOverride(Dictionary<string, Dictionary<string, TurretOrientation>> ArtilleryTurretOverrides);
}