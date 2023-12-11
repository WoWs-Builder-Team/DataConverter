using System;
using System.Collections.Immutable;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public sealed class Ship
{
    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int Tier { get; init; }

    public ShipClass ShipClass { get; init; }

    public ShipCategory ShipCategory { get; init; }

    public Nation ShipNation { get; init; }

    [Obsolete("Ship camos are irrelevant for shipbuilder")]
    public ImmutableList<string> Permoflages { get; init; } = ImmutableList<string>.Empty;

    public ImmutableDictionary<string, TurretModule> MainBatteryModuleList { get; init; } = ImmutableDictionary<string, TurretModule>.Empty;

    public ImmutableDictionary<string, FireControl> FireControlList { get; init; } = ImmutableDictionary<string, FireControl>.Empty;

    public ImmutableDictionary<string, TorpedoModule> TorpedoModules { get; init; } = ImmutableDictionary<string, TorpedoModule>.Empty;

    public ImmutableDictionary<string, Engine> Engines { get; init; } = ImmutableDictionary<string, Engine>.Empty;

    public ImmutableDictionary<string, Hull> Hulls { get; init; } = ImmutableDictionary<string, Hull>.Empty;

    public ImmutableDictionary<string, ImmutableArray<string>> CvPlanes { get; init; } = ImmutableDictionary<string, ImmutableArray<string>>.Empty;

    public ImmutableDictionary<string, AirStrike> AirStrikes { get; init; } = ImmutableDictionary<string, AirStrike>.Empty;

    //may not need to be a List, but possibly an upgradeable module
    public ImmutableDictionary<string, PingerGun> PingerGunList { get; init; } = ImmutableDictionary<string, PingerGun>.Empty;

    public ImmutableList<ShipConsumable> ShipConsumable { get; init; } = ImmutableList<ShipConsumable>.Empty;

    public UpgradeInfo ShipUpgradeInfo { get; init; } = new();

    public SpecialAbility? SpecialAbility { get; init; }

    public ImmutableDictionary<string, ShellCompatibility> ShellCompatibilities { get; init; } = ImmutableDictionary<string, ShellCompatibility>.Empty;
}
