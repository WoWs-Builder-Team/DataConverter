using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures
{
    public class ShipSummary
    {
        public ShipSummary(string index, Nation nation, int tier, ShipClass shipClass, ShipCategory category, string prevShipIndex, List<string> nextShipsIndex)
        {
            Index = index;
            Nation = nation;
            Tier = tier;
            ShipClass = shipClass;
            Category = category;
            PrevShipIndex = prevShipIndex;
            NextShipsIndex = nextShipsIndex ?? new List<string>();
        }
        
        public string Index { get; }
        
        public Nation Nation { get; }
        
        public int Tier { get; }
        
        public string PrevShipIndex { get; }

        public List<string> NextShipsIndex { get; }

        public ShipClass ShipClass { get; }
        
        public ShipCategory Category { get; }
    }
}