using System.Collections.Generic;
using WoWsShipBuilderDataStructures;

namespace DataConverter.WGStructure
{
    public class ShipTurretOverride
    {
        public ShipTurretOverride(Dictionary<string, Dictionary<string, TurretOrientation>> turretOverrides)
        {
            ArtilleryTurretOverrides = turretOverrides;
        }
        
        /// <summary>
        /// Maps the name of an artillery module to a dictionary mapping the turret keys to their position overrides.
        /// </summary>
        public Dictionary<string, Dictionary<string, TurretOrientation>> ArtilleryTurretOverrides { get; }
    }
}