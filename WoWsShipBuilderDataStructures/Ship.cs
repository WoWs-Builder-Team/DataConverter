using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWsShipBuilderDataStructures
{
    class Ship
    {
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public int Tier { get; set; }

        public List<TurretModule> MainBatteryModuleList { get; set; }
        public List<FireControl> FireControlList { get; set; }
        public List<TorpedoLauncher> TorpedoLauncherList { get; set; }
        public AntiAir AntiAir { get; set; }
        public List<TurretModule> SecondariesList { get; set; }
        public DepthChargeArray DepthChargeArray { get; set; }
        public List<Engine> Engines { get; set; }
        public List<Hull> Hulls { get; set; }
        public List<PlaneType> CVPlaneTypes {get; set;}
        //may not need to be a List, but possibly an upgradeable module
        public List<PingerGun> PingerGunList { get; set; }

        public List<ShipConsumable> ShipConsumable { get; set; }
    }

    #region Main Battery and Secondaries
    //small abuse, but we reuse this for secondaries.
    public class TurretModule
    {
        public float Sigma { get; set; }
        public float MaxRange { get; set; }
        public List<Gun> Guns { get; set; }
        public ModuleTier ModuleTier { get; set; }
    }

    public class Gun
    {
        public List<string> AmmoList { get; set; }
        public float BarrelDiameter { get; set; }
        public float[] HorizontalSector { get; set; }
        public long Id { get; set; }
        public string Index { get; set; }
        public int NumBarrels { get; set; }
        public float HorizontalPosition { get; set; }
        public float VerticalPosition { get; set; }
        public float HorizontalRotationSpeed { get; set; }
        public float VerticalRotationSpeed { get; set; }
        public float Reload { get; set; }
        public float SmokeDetectionWhenFiring { get; set; }
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
        public float[] torpedoAngles { get; set; }
        public ModuleTier ModuleTier { get; set; }
    }

    #endregion

    #region AA
    public class AntiAir{
        public AntiAirAura LongRangeAura { get; set; }
        public AntiAirAura MediumRangeAura { get; set; }
        public AntiAirAura ShortRangeAura { get; set; }
    }

    public class AntiAirAura
    {
        public float ConstantDPS { get; set; }
        public const float DAMAGE_INTERVAL = 0.285714285714F;
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
    //copy of WG value. No idea of what we need or what is ufseful
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
        public string UcType { get; set; }
        public bool CanBuy { get; set; }
    }
    #endregion
}
