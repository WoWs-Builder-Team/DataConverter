namespace WoWsShipBuilderDataStructures
{
    public class Bomb 
    { 
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public ProjectileType ProjectileType { get; set; }
        public float Damage { get; set; }
        public float Penetration { get; set; }
        public BombType BombType { get; set; }
        public float AirDrag { get; set; }
        public float FuseTimer { get; set; }
        public float ArmingThreshold { get; set; }
        public float Caliber { get; set; }
        public float Krupp { get; set; }
        public float Mass { get; set; }
        public float RicochetAngle { get; set; }
        public float MuzzleVelocity { get; set; }
        public float FireChance { get; set; }

    }
}
