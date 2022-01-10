using System.Collections.Generic;

#nullable enable
namespace WoWsShipBuilder.DataStructures
{
    /// <summary>
    /// Container class for turret position overrides.
    /// </summary>
    /// <param name="ArtilleryTurretOverrides">Maps the name of an artillery module to a dictionary mapping the turret keys to their position overrides.</param>
    public sealed record ShipTurretOverride(Dictionary<string, Dictionary<string, TurretOrientation>> ArtilleryTurretOverrides);
}
#nullable restore
