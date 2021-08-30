using System.Collections.Generic;
// ReSharper disable ClassNeverInstantiated.Global

namespace WoWsShipBuilderDataStructures
{
    public class Ship
    {
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public int Tier { get; set; }

        public Dictionary<string, TurretModule> MainBatteryModuleList { get; set; }
        public Dictionary<string, FireControl> FireControlList { get; set; }
        public Dictionary<string, TorpedoLauncher> TorpedoLauncherList { get; set; }
        // TODO: does this really work?
        public AntiAir AntiAir { get; set; }
        public Dictionary<string, TurretModule> SecondariesList { get; set; }
        public DepthChargeArray DepthChargeArray { get; set; }
        public Dictionary<string, Engine> Engines { get; set; }
        public Dictionary<string, Hull> Hulls { get; set; }
        public Dictionary<string, PlaneType> CvPlaneTypes { get; set; }
        public Dictionary<string, AirStrike> AirStrikes { get; set; }
        public List<string> BomberPlaneNames { get; set; }
        public List<string> TorpedoPlaneNames { get; set; }
        public List<string> SkipPlaneNames { get; set; }

        public List<string> RocketPlaneNames { get; set; }

        //may not need to be a List, but possibly an upgradeable module
        public List<PingerGun> PingerGunList { get; set; }
        public List<ShipConsumable> ShipConsumable { get; set; }
        public List<ShipUpgrades> ShipUpgradeInfo { get; set; }
    }

    #region Main Battery and Secondaries

    //small abuse, but we reuse this for secondaries.
    public class TurretModule
    {
        public double Sigma { get; set; }
        public double MaxRange { get; set; }
        public List<Gun> Guns { get; set; }
        public ModuleTier ModuleTier { get; set; }
    }

    public class Gun
    {
        public List<string> AmmoList { get; set; }
        public double BarrelDiameter { get; set; }
        public double[] HorizontalSector { get; set; }
        public double[][] HorizontalDeadZones { get; set; }
        public long Id { get; set; }
        public string Index { get; set; }
        public int NumBarrels { get; set; }
        public double HorizontalPosition { get; set; }
        public double VerticalPosition { get; set; }
        public double HorizontalRotationSpeed { get; set; }
        public double VerticalRotationSpeed { get; set; }
        public double Reload { get; set; }
        public double SmokeDetectionWhenFiring { get; set; }
    }

    #endregion

    #region Fire Control

    public class FireControl
    {
        public float MaxRangeModifier { get; set; }
        public float SigmaModifier { get; set; }
        public ModuleTier ModuleTier { get; set; }
    }

    #endregion

    #region Torpedo

    public class TorpedoLauncher
    {
        public List<string> AmmoList { get; set; }
        public float BarrelDiameter { get; set; }
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public int NumBarrels { get; set; }
        public float HorizontalRotationSpeed { get; set; }
        public float VerticalRotationSpeed { get; set; }
        public float Reload { get; set; }
        public float[] TorpedoAngles { get; set; }
        public ModuleTier ModuleTier { get; set; }
    }

    #endregion

    #region AirStrike

    public class AirStrike
    {
        public int Charges { get; set; }
        public double FlyAwayTime { get; set; }
        public int MaximumDistance { get; set; }
        public int MaximumFlightDistance { get; set; }
        public int MinimumDistance { get; set; }
        public string PlaneName { get; set; }
        public double ReloadTime { get; set; }
        public double TimeBetweenShots { get; set; }
        public double DropTime { get; set; }
    }

    #endregion

    #region AA

    public class AntiAir
    {
        public AntiAirAura LongRangeAura { get; set; }
        public AntiAirAura MediumRangeAura { get; set; }
        public AntiAirAura ShortRangeAura { get; set; }
    }

    public class AntiAirAura
    {
        public float ConstantDps { get; set; }
        public const float DamageInterval = 0.285714285714F;
        public float FlakDamage { get; set; }
        public int FlakCloudsNumber { get; set; }
        public float HitChance { get; set; }
        public float MaxRange { get; set; }
        public float MinRange { get; set; }
    }

    #endregion

    #region Depth Charges

    public class DepthChargeArray
    {
        public int MaxPacks { get; set; }
        public float Reload { get; set; }
        public List<DepthCharge> DepthCharges { get; set; }
    }

    public class DepthCharge
    {
        public List<string> AmmoList { get; set; }
        public float[] HorizontalSector { get; set; }
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public int DepthChargesNumber { get; set; }
        public float[] RotationSpeed { get; set; }
    }

    #endregion

    #region Engine

    public class Engine
    {
        public float BackwardEngineUpTime { get; set; }
        public float ForwardEngineUpTime { get; set; }
        public float SpeedCoef { get; set; }
        public ModuleTier ModuleTier { get; set; }
    }

    #endregion

    #region Hull

    public class Hull
    {
        public float Health { get; set; }
        public float MaxSpeed { get; set; }
        public float RudderTime { get; set; }
        public float SpeedCoef { get; set; }
        public float SmokeFiringDetection { get; set; }
        public float SurfaceDetection { get; set; }
        public float AirDetection { get; set; }
    }

    #endregion

    #region PingerGun

    //copy of WG value. No idea of what we need or what is useful
    public class PingerGun
    {
        public float[] RotationSpeed { get; set; }
        public SectorParam[] SectorParams { get; set; }
        public float WaveDistance { get; set; }
        public int WaveHitAlertTime { get; set; }
        public int WaveHitLifeTime { get; set; }
        public WaveParam[] WaveParams { get; set; }
        public float WaveReloadTime { get; set; }
    }

    public class SectorParam
    {
        public float AlertTime { get; set; }
        public float Lifetime { get; set; }
        public float Width { get; set; }
        public float[][] WidthParams { get; set; }
    }

    public class WaveParam
    {
        public float EndWaveWidth { get; set; }
        public float EnergyCost { get; set; }
        public float StartWaveWidth { get; set; }
        public float[] WaveSpeed { get; set; }
    }

    #endregion

    #region Ship Consumables

    public class ShipConsumable
    {
        public int Slot { get; set; }
        public string ConsumableName { get; set; }
        public string ConsumableVariantName { get; set; }
    }

    #endregion

    #region Ship Upgrades and Modules

    //pretty much a copy of WG structure
    public class ShipUpgrades
    {
        public Dictionary<string, string[]> Components { get; set; }
        public object[] NextShips { get; set; }
        public string Prev { get; set; }
        public ModuleType UcType { get; set; }
        public bool CanBuy { get; set; }
    }

    #endregion
}