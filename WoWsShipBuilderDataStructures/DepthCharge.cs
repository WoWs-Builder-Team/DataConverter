namespace WoWsShipBuilderDataStructures
{
    public class DepthCharge
    { 
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public ProjectileType ProjectileType { get; set; }
        public float Damage { get; set; }
        public float FireChance { get; set; }
        public float FloodChance { get; set; }
        public float DetonationTimer { get; set; }
        public float SinkingSpeed { get; set; }
    }
}
