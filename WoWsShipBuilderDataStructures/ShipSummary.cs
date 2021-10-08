namespace WoWsShipBuilderDataStructures
{
    public class ShipSummary
    {
        public ShipSummary(string index, Nation nation, int tier, ShipClass shipClass, ShipCategory category)
        {
            Index = index;
            Nation = nation;
            Tier = tier;
            ShipClass = shipClass;
            Category = category;
        }
        
        public string Index { get; }
        
        public Nation Nation { get; }
        
        public int Tier { get; }
        
        public ShipClass ShipClass { get; }
        
        public ShipCategory Category { get; }
    }
}