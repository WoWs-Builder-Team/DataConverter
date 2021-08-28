using System.Collections.Generic;

namespace WoWsShipBuilderDataStructures
{
    public class Torpedo
    { 
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public ProjectileType ProjectileType { get; set; }
        public float SpottingRange { get; set; } //It's visibilityFactor
        public float Damage { get; set; } //(alphaDamage/3)+damage 
        public TorpedoType TorpedoType { get; set; }
        public float Caliber { get; set; }
        public float MaxRange { get; set; }//MaxDist*30
        public List<ShipClass> IgnoreClasses { get; set; }
        public float Speed { get; set; }
        public float ArmingTime { get; set; }
        public float FloodChance { get; set; } // It's uwCritical
    }
}
