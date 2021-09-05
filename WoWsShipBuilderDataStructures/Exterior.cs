using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsShipBuilderDataStructures
{
    public class Exterior
    {
        public long Id { get; set; }
        public string Index { get; set; }
        public Dictionary<string,double> Modifiers { get; set; }
        public string Name { get; set; }
        public ExteriorType Type { get; set; }
        public int SortOrder { get; set; }
        public Restriction Restrictions { get; set; }
    }


    public class Restriction
    {
        public List<string> ForbiddenShips { get; set; }
        public List<string> Levels { get; set; }
        public List<string> Nations { get; set; }
        public List<string> SpecificShips { get; set; }
        public List<string> Subtype { get; set; }
    }
}
