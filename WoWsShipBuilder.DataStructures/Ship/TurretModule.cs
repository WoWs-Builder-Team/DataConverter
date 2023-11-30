using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class TurretModule
{
    public decimal Sigma { get; set; }

    public decimal MaxRange { get; set; }

    public ImmutableArray<Gun> Guns { get; set; } = ImmutableArray<Gun>.Empty;

    public AntiAirAura? AntiAir { get; set; }

    public BurstModeAbility? BurstModeAbility { get; set; }
}
