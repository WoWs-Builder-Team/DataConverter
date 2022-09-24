using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class TurretModule
{
    public decimal Sigma { get; set; }

    public decimal MaxRange { get; set; }

    public List<Gun> Guns { get; set; } = new();

    public AntiAirAura? AntiAir { get; set; }

    public Dispersion DispersionValues { get; set; } = new();

    public BurstModeAbility? BurstModeAbility { get; set; }
}
