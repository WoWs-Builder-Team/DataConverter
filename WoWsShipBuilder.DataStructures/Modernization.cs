using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures
{
    public class Modernization
    {
        public long Id { get; set; }
        public string Index { get; set; }
        public Dictionary<string, double> Effect { get; set; }
        public string Name { get; set; }
        public List<Nation> AllowedNations { get; set; }
        public List<int> ShipLevel { get; set; }
        public List<string> AdditionalShips { get; set; }
        public List<ShipClass> ShipClasses { get; set; }
        public int Slot { get; set; }
        public List<string> BlacklistedShips { get; set; }
        public ModernizationType Type { get; set; }
    }

}
