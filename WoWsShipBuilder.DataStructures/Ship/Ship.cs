using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class Ship
{
    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int Tier { get; init; }

    public ShipClass ShipClass { get; init; }

    public ShipCategory ShipCategory { get; init; }

    public Nation ShipNation { get; init; }

    public List<string> Permoflages { get; set; } = new();

    public Dictionary<string, TurretModule> MainBatteryModuleList { get; set; } = new();

    public Dictionary<string, FireControl> FireControlList { get; set; } = new();

    public Dictionary<string, TorpedoModule> TorpedoModules { get; set; } = new();

    public Dictionary<string, Engine> Engines { get; set; } = new();

    public Dictionary<string, Hull> Hulls { get; set; } = new();

    public Dictionary<string, List<string>> CvPlanes { get; set; } = new();

    public Dictionary<string, AirStrike> AirStrikes { get; set; } = new();

    //may not need to be a List, but possibly an upgradeable module
    public Dictionary<string, PingerGun> PingerGunList { get; set; } = new();

    public List<ShipConsumable> ShipConsumable { get; set; } = new();

    public UpgradeInfo ShipUpgradeInfo { get; set; } = new();

    public SpecialAbility? SpecialAbility { get; set; }

    public Dictionary<string, ShellCompatibility> ShellCompatibilities { get; set; } = new();
}
