using System.Collections.Generic;

namespace DataConverter.WGStructure
{

    public class WGExterior
    {
        public long id { get; set; }
        public string index { get; set; }
        public Dictionary<string, double> modifiers { get; set; }
        public string name { get; set; }
        public Typeinfo typeinfo { get; set; }
        public int sortOrder { get; set; }
        public Restriction restrictions { get; set; }
    }

    //check if they are actually string, couldn't find one example
    public class Restriction
    {
        public string[] forbiddenShips { get; set; }
        public string[] levels { get; set; }
        public string[] nations { get; set; }
        public string[] specificShips { get; set; }
        public string[] subtype { get; set; }
    }

}
