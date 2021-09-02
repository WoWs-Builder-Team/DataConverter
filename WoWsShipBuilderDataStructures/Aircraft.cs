using System.Collections.Generic;

namespace WoWsShipBuilderDataStructures
{
    public class Aircraft
    {
        public long Id { get; set; }
        public string Index { get; set; }
        public float MaxHealth { get; set; }
        public string Name { get; set; }
        public int NumPlanesInSquadron { get; set; }
        public float ReturnHeight { get; set; }
        public float SpeedMaxModifier { get; set; }
        public float SpeedMinModifier { get; set; }
        public float Speed { get; set; }
        public double NaturalAcceleration { get; set; }
        public double NaturalDeceleration { get; set; }
        
        public int MaxPlaneInHangar { get; set; }
        public int StartingPlanes { get; set; }
        public float RestorationTime { get; set; }
        
        public double BombFallingTime { get; set; }
        public string BombName { get; set; }
        public double DamageTakenMultiplier { get; set; }
        public double FlightHeight { get; set; }
        public double FlightRadius { get; set; }
        public double InnerBombsPercentage { get; set; }
        public List<double> InnerSalvoSize { get; set; }
        
        public PlaneCategory PlaneCategory { get; set; }
        public PlaneAttackData AttackData { get; set; }
        public JatoData JatoData { get; set; }

    }

    public class PlaneAttackData
    {
        public double AttackCooldown { get; set; }
        public int AttackCount { get; set; }
        public double AttackInterval { get; set; }
        public double AttackSpeedMultiplier { get; set; }
        public double AttackSpeedMultiplierApplyTime { get; set; }
        public double AttackerDamageTakenMultiplier { get; set; }
        public int AttackerSize { get; set; }
    }

    public class JatoData
    {
        public double JatoDuration { get; set; }
        public double JatoSpeedMultiplier { get; set; }
    }
}
