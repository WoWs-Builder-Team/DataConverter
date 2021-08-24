using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsShipBuilderDataStructures
{
    class Module
    {
        public int CostCr { get; set; }
        public int CostXP { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public ModuleType Type { get; set; }
    }
}
