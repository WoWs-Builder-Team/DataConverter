using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace GameParamsExtractor.WGStructure
{
    public class WgShip : WGObject
    {
        public Dictionary<string, ModuleArmaments> ModulesArmaments { get; set; } = new();

        public Dictionary<string, ShipAbility> ShipAbilities { get; set; } = new();

        public ShipUpgradeInfo ShipUpgradeInfo { get; set; } = new();

        public long Id { get; set; }

        public string Index { get; set; } = string.Empty;

        public int Level { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Group { get; set; } = string.Empty;

        public List<string> Permoflages { get; set; } = new();
    }

    [JsonConverter(typeof(JsonSubtypes))]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(MainBattery), "guns")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(WgFireControl), "maxDistCoef")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(WgTorpedoArray), "torpedoArray")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(Atba), "antiAirAndSecondaries")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(AirSupport), "maxPlaneFlightDist")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(AirDefense), "isAA")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(WgDepthChargesArray), "depthCharges")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(WgEngine), "forwardEngineUpTime")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(WgHull), "visibilityFactor")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(WgFlightControl), "squadrons")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(WgPingerGun), "waveReloadTime")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(WgPlane), "planeType")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(WgPlane), "planes")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(WgSpecialAbility), "RageMode")]
    public class ModuleArmaments
    {
    }

    #region Main battery

    public class MainBattery : ModuleArmaments
    {
        public Dictionary<string, MainBatteryGun> Guns { get; set; } = new();

        public BurstArtilleryModule? BurstArtilleryModule { get; set; }

        public decimal MaxDist { get; set; }

        public decimal SigmaCount { get; set; }

        public double TaperDist { get; set; }

        public bool NormalDistribution { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Other { get; set; } = new();

        [JsonIgnore]
        public Dictionary<string, AaAura> AntiAirAuras =>
            Other.Select(entry => new KeyValuePair<string, AaAura?>(entry.Key, entry.Value.ToObjectSafe<AaAura>()))
                .Where(entry => entry.Value is not null)
                .ToDictionary(entry => entry.Key, entry => entry.Value!);
    }

    public class MainBatteryGun
    {
        public string[] AmmoList { get; set; } = Array.Empty<string>();

        public decimal BarrelDiameter { get; set; }

        public double[] HorizSector { get; set; } = Array.Empty<double>();

        public long Id { get; set; }

        public string Index { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int NumBarrels { get; set; }

        public double[] Position { get; set; } = Array.Empty<double>();

        public decimal[] RotationSpeed { get; set; } = Array.Empty<decimal>();

        public double[][] DeadZone { get; set; } = { };

        public decimal ShotDelay { get; set; }

        public decimal SmokePenalty { get; set; }

        public double IdealRadius { get; set; }

        public double MinRadius { get; set; }

        public double IdealDistance { get; set; }

        public double RadiusOnZero { get; set; }

        public double RadiusOnDelim { get; set; }

        public double RadiusOnMax { get; set; }

        public double Delim { get; set; }

        public TypeInfo TypeInfo { get; set; } = new();

        public static explicit operator Gun(MainBatteryGun gun)
        {
            var newGun = new Gun
            {
                AmmoList = gun.AmmoList.ToList(),
                BarrelDiameter = gun.BarrelDiameter,
                HorizontalSector = gun.HorizSector,
                HorizontalDeadZones = gun.DeadZone,
                Id = gun.Id,
                Index = gun.Index,
                Name = gun.Name,
                NumBarrels = gun.NumBarrels,
                HorizontalPosition = gun.Position[1],
                VerticalPosition = gun.Position[0],
                HorizontalRotationSpeed = gun.RotationSpeed[0],
                VerticalRotationSpeed = gun.RotationSpeed[1],
                Reload = gun.ShotDelay,
                SmokeDetectionWhenFiring = gun.SmokePenalty,
            };
            newGun.TurretOrientation = newGun.VerticalPosition < 3 ? TurretOrientation.Forward : TurretOrientation.Backward;

            return newGun;
        }
    }

    #endregion

    #region Fire Control

    public class WgFireControl : ModuleArmaments
    {
        public decimal MaxDistCoef { get; set; }

        public decimal SigmaCountCoef { get; set; }
    }

    #endregion

    #region Torpedos

    public class WgTorpedoArray : ModuleArmaments
    {
        public decimal TimeToChangeAmmo { get; set; }

        public Dictionary<string, WgTorpedoLauncher> TorpedoArray { get; set; } = new();
    }

    public class WgTorpedoLauncher
    {
        public string[] AmmoList { get; set; } = Array.Empty<string>();

        public decimal BarrelDiameter { get; set; }

        public decimal[][] DeadZone { get; set; } = Array.Empty<decimal[]>();

        public decimal[] HorizSector { get; set; } = Array.Empty<decimal>();

        public long Id { get; set; }

        public string Index { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int NumBarrels { get; set; }

        public decimal[] RotationSpeed { get; set; } = Array.Empty<decimal>();

        public decimal ShotDelay { get; set; }

        public decimal[] TorpedoAngles { get; set; } = Array.Empty<decimal>(); //unknonw meaning, needed?

        public TypeInfo TypeInfo { get; set; } = new();

        public static implicit operator TorpedoLauncher(WgTorpedoLauncher thisLauncher)
        {
            return new()
            {
                AmmoList = thisLauncher.AmmoList.ToList(),
                BarrelDiameter = thisLauncher.BarrelDiameter,
                HorizontalRotationSpeed = thisLauncher.RotationSpeed[0],
                VerticalRotationSpeed = thisLauncher.RotationSpeed[1],
                Id = thisLauncher.Id,
                Index = thisLauncher.Index,
                Name = thisLauncher.Name,
                NumBarrels = thisLauncher.NumBarrels,
                Reload = thisLauncher.ShotDelay,
                HorizontalSector = thisLauncher.HorizSector,
                HorizontalDeadZone = thisLauncher.DeadZone,
                TorpedoAngles = thisLauncher.TorpedoAngles,
            };
        }
    }

    #endregion

    #region AntiAir and secondaries

    //this is AA and secondaries too. smallGun i think indicates if it's a secondary
    public class Atba : ModuleArmaments
    {
        public Dictionary<string, AntiAirAndSecondaries> AntiAirAndSecondaries { get; set; } = new();

        public decimal MaxDist { get; set; }

        public decimal SigmaCount { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Other { get; set; } = new();

        [JsonIgnore]
        public Dictionary<string, AaAura> AntiAirAuras =>
            Other.Select(entry => new KeyValuePair<string, AaAura?>(entry.Key, entry.Value.ToObjectSafe<AaAura>()))
                .Where(entry => entry.Value is not null)
                .ToDictionary(entry => entry.Key, entry => entry.Value!);
    }

    public class AntiAirAndSecondaries
    {
        public string[] AmmoList { get; set; } = Array.Empty<string>();

        public long Id { get; set; }

        public string Index { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal BarrelDiameter { get; set; }

        public int NumBarrels { get; set; }

        public decimal[] RotationSpeed { get; set; } = Array.Empty<decimal>();

        public decimal ShotDelay { get; set; }

        public bool SmallGun { get; set; }

        public TypeInfo TypeInfo { get; set; } = new();

        public static explicit operator Gun(AntiAirAndSecondaries wgSecondary)
        {
            return new Gun
            {
                AmmoList = wgSecondary.AmmoList.ToList(),
                BarrelDiameter = wgSecondary.BarrelDiameter,
                Id = wgSecondary.Id,
                Index = wgSecondary.Index,
                Name = wgSecondary.Name,
                NumBarrels = wgSecondary.NumBarrels,
                HorizontalRotationSpeed = wgSecondary.RotationSpeed[0],
                VerticalRotationSpeed = wgSecondary.RotationSpeed[1],
                Reload = wgSecondary.ShotDelay,
            };
        }
    }

    #endregion

    #region AirSupport

    /// <summary>
    /// Data for air strikes of ships.
    /// </summary>
    public class AirSupport : ModuleArmaments
    {
        public int ChargesNum { get; set; }

        public decimal FlyAwayTime { get; set; }

        public int MaxDist { get; set; }

        public int MaxPlaneFlightDist { get; set; }

        public int MinDist { get; set; }

        public string PlaneName { get; set; } = string.Empty;

        public decimal ReloadTime { get; set; }

        public decimal TimeBetweenShots { get; set; }

        public decimal TimeFromHeaven { get; set; }

        public static implicit operator AirStrike(AirSupport thisAirSupport)
        {
            return new AirStrike
            {
                Charges = thisAirSupport.ChargesNum,
                FlyAwayTime = thisAirSupport.FlyAwayTime,
                MaximumDistance = thisAirSupport.MaxDist,
                MaximumFlightDistance = thisAirSupport.MaxPlaneFlightDist,
                MinimumDistance = thisAirSupport.MinDist,
                PlaneName = thisAirSupport.PlaneName,
                DropTime = thisAirSupport.TimeFromHeaven,
                ReloadTime = thisAirSupport.ReloadTime,
                TimeBetweenShots = thisAirSupport.TimeBetweenShots,
            };
        }
    }

    #endregion

    #region Air Defense

    public class AirDefense : ModuleArmaments
    {
        public bool IsAa { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Other { get; set; } = new();

        [JsonIgnore]
        public Dictionary<string, AaAura> AntiAirAuras =>
            Other.Select(entry => new KeyValuePair<string, AaAura?>(entry.Key, entry.Value.ToObjectSafe<AaAura>()))
                .Where(entry => entry.Value is not null)
                .ToDictionary(entry => entry.Key, entry => entry.Value!);
    }

    public class AaAura
    {
        public decimal AreaDamage { get; set; }

        public decimal AreaDamagePeriod { get; set; }

        public decimal BubbleDamage { get; set; }

        [JsonRequired]
        public decimal HitChance { get; set; }

        public int InnerBubbleCount { get; set; }

        public decimal MaxDistance { get; set; }

        public decimal MinDistance { get; set; }

        public string Type { get; set; } = string.Empty;

        public static implicit operator AntiAirAura(AaAura aura)
        {
            return new AntiAirAura
            {
                ConstantDps = aura.AreaDamage,
                FlakDamage = aura.BubbleDamage,
                FlakCloudsNumber = aura.InnerBubbleCount,
                HitChance = aura.HitChance,
                MaxRange = aura.MaxDistance,
                MinRange = aura.MinDistance,
            };
        }
    }

    #endregion

    #region DepthCharges

    public class WgDepthChargesArray : ModuleArmaments
    {
        public Dictionary<string, WgDepthChargeLauncher> DepthCharges { get; set; } = new();

        public int MaxPacks { get; set; }

        public int NumShots { get; set; }

        public decimal ReloadTime { get; set; }
    }

    public class WgDepthChargeLauncher
    {
        public string[] AmmoList { get; set; } = Array.Empty<string>();

        public decimal[] HorizSector { get; set; } = Array.Empty<decimal>();

        public long Id { get; set; }

        public string Index { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int NumBombs { get; set; }

        public decimal[] RotationSpeed { get; set; } = Array.Empty<decimal>();

        public TypeInfo TypeInfo { get; set; } = new();

        public static implicit operator DepthChargeLauncher(WgDepthChargeLauncher wgLauncher)
        {
            return new DepthChargeLauncher
            {
                AmmoList = wgLauncher.AmmoList.ToList(),
                DepthChargesNumber = wgLauncher.NumBombs,
                HorizontalSector = wgLauncher.HorizSector,
                Id = wgLauncher.Id,
                Index = wgLauncher.Index,
                Name = wgLauncher.Name,
                RotationSpeed = wgLauncher.RotationSpeed,
            };
        }
    }

    #endregion

    #region Engine

    public class WgEngine : ModuleArmaments
    {
        public decimal BackwardEngineUpTime { get; set; }

        public decimal ForwardEngineUpTime { get; set; }

        public decimal SpeedCoef { get; set; }

        public HitLocationEngine HitLocationEngine { get; set; } = new();
    }

    public class HitLocationEngine
    {
        public decimal ArmorCoeff { get; set; }
    }

    #endregion

    #region Hull

    public class WgHull : ModuleArmaments
    {
        public decimal Health { get; set; }

        public decimal MaxSpeed { get; set; }

        public decimal RudderTime { get; set; }

        public SteeringGear Sg { get; set; } = new();

        public decimal SpeedCoef { get; set; }

        public decimal VisibilityCoefGkInSmoke { get; set; }

        public decimal VisibilityFactor { get; set; }

        public decimal VisibilityFactorByPlane { get; set; }

        public Dictionary<string, decimal> VisibilityFactorsBySubmarine { get; set; } = new();

        public decimal[][] BurnNodes { get; set; } = Array.Empty<decimal[]>(); // Format: Fire resistance coeff, damage per second in %, burn time in s

        public decimal[][] FloodNodes { get; set; } = Array.Empty<decimal[]>(); // Format: Torpedo belt reduction, damage per second in %, flood time in s

        public int TurningRadius { get; set; }

        public decimal[] Size { get; set; } = Array.Empty<decimal>();
    }

    #endregion

    #region Steering Gear

    public class SteeringGear
    {
        public decimal ArmorCoeff { get; set; }
    }

    #endregion

    #region FlightControl

    public class WgFlightControl : ModuleArmaments
    {
        public string[] Squadrons { get; set; } = Array.Empty<string>();
    }

#nullable enable
    public class WgPlane : ModuleArmaments
    {
        public string? PlaneType { get; set; }

        public string[]? Planes { get; set; }
    }
#nullable disable

    #endregion

    #region Sub Pinger Gun

    public class WgPingerGun : ModuleArmaments
    {
        public decimal[] RotationSpeed { get; set; }

        public WgSectorParam[] SectorParams { get; set; }

        public decimal WaveDistance { get; set; }

        public int WaveHitAlertTime { get; set; }

        public int WaveHitLifeTime { get; set; }

        public WgWaveParam[] WaveParams { get; set; }

        public decimal WaveReloadTime { get; set; }

        public static implicit operator PingerGun(WgPingerGun thisWgPingerGun)
        {
            return new PingerGun
            {
                RotationSpeed = thisWgPingerGun.RotationSpeed,
                SectorParams = thisWgPingerGun.SectorParams.Select(wgSectorParam => (SectorParam)wgSectorParam).ToArray(),
                WaveDistance = thisWgPingerGun.WaveDistance,
                WaveHitAlertTime = thisWgPingerGun.WaveHitAlertTime,
                WaveHitLifeTime = thisWgPingerGun.WaveHitLifeTime,
                WaveParams = thisWgPingerGun.WaveParams.Select(wgWaveParam => (WaveParam)wgWaveParam).ToArray(),
                WaveReloadTime = thisWgPingerGun.WaveReloadTime,
            };
        }
    }

    public class WgSectorParam
    {
        public decimal AlertTime { get; set; }

        public decimal Lifetime { get; set; }

        public decimal Width { get; set; }

        public decimal[][] WidthParams { get; set; }

        public static implicit operator SectorParam(WgSectorParam thisSectorParam)
        {
            return new SectorParam
            {
                AlertTime = thisSectorParam.AlertTime,
                Lifetime = thisSectorParam.Lifetime,
                Width = thisSectorParam.Width,
                WidthParams = thisSectorParam.WidthParams,
            };
        }
    }

    public class WgWaveParam
    {
        public decimal EndWaveWidth { get; set; }

        public decimal EnergyCost { get; set; }

        public decimal StartWaveWidth { get; set; }

        public decimal[] WaveSpeed { get; set; }

        public static implicit operator WaveParam(WgWaveParam thisWaveParam)
        {
            return new WaveParam
            {
                EndWaveWidth = thisWaveParam.EndWaveWidth,
                EnergyCost = thisWaveParam.EnergyCost,
                StartWaveWidth = thisWaveParam.StartWaveWidth,
                WaveSpeed = thisWaveParam.WaveSpeed,
            };
        }
    }

    #endregion

    #region Ship Consumable

    public class ShipAbility
    {
        public string[][] Abils { get; set; }

        public int Slot { get; set; }
    }

    #endregion

    #region Ship Upgrades and modules

    public class ShipUpgradeInfo
    {
        public int CostCr { get; set; }

        public int CostGold { get; set; }

        public int CostSaleGold { get; set; }

        public int CostXp { get; set; }

        public int Value { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> Other { get; set; }

        [JsonIgnore]
        public Dictionary<string, ShipUpgrade> ConvertedUpgrades =>
            Other.Select(entry => (entry.Key, entry.Value.ToObjectSafe<ShipUpgrade>()))
                .Where(entry => entry.Item2 is not null)
                .ToDictionary(entry => entry.Key, entry => entry.Item2);
    }

    public class ShipUpgrade
    {
        public bool CanBuy { get; set; }

        public Dictionary<string, string[]> Components { get; set; }

        public string[] NextShips { get; set; }

        public string Prev { get; set; }

        public string UcType { get; set; }
    }

    #endregion

    #region Special abilities of super ship

    public class BurstArtilleryModule
    {
        public decimal BurstReloadTime { get; set; }

        public decimal FullReloadTime { get; set; }

        public Dictionary<string, float> Modifiers { get; set; }

        public int ShotsCount { get; set; }
    }

    public class WgSpecialAbility : ModuleArmaments
    {
        public RageMode RageMode { get; set; }

        public int BuffsShiftMaxLevel { get; set; }

        public double BuffsShiftSpeed { get; set; }

        public int BuffsStartPool { get; set; }

        public Dictionary<string, float> Modifiers { get; set; }
    }

    public class RageMode
    {
        public double BoostDuration { get; set; }

        public int DecrementCount { get; set; }

        public double DecrementDelay { get; set; }

        public double DecrementPeriod { get; set; }

        public int GunsForSalvo { get; set; }

        public Dictionary<string, float> Modifiers { get; set; }

        public double Radius { get; set; }

        public string RageModeName { get; set; }

        public int RequiredHits { get; set; }
    }

    #endregion
}
