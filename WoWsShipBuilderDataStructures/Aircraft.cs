using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsShipBuilderDataStructures
{
    class Aircraft
    {
        public long Id { get; set; }
        public string Index { get; set; }
        public int MaxHealth { get; set; }
        public string Name { get; set; }
        public int NumPlanesInSquadron { get; set; }
        public float ReturnHeight { get; set; }
        public float SpeedMaxModifier { get; set; }
        public float SpeedMinModifier { get; set; }
        public float Speed { get; set; }
        public int MaxPlaneInHangar { get; set; }
        public int StartingPlanes { get; set; }
        public float RestorationTime { get; set; }

    }
}
