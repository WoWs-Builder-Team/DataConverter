using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Ship.Components;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public sealed class Gun : IGun
{
    public ImmutableArray<string> AmmoList { get; init; } = ImmutableArray<string>.Empty;

    public decimal BarrelDiameter { get; init; }

    public ImmutableArray<decimal> HorizontalSector { get; init; } = ImmutableArray<decimal>.Empty;

    public ImmutableArray<ImmutableArray<decimal>> HorizontalDeadZones { get; init; } = ImmutableArray<ImmutableArray<decimal>>.Empty;

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int NumBarrels { get; init; }

    public decimal HorizontalPosition { get; init; }

    public decimal VerticalPosition { get; init; }

    public decimal HorizontalRotationSpeed { get; init; }

    public decimal VerticalRotationSpeed { get; init; }

    public decimal Reload { get; init; }

    public decimal SmokeDetectionWhenFiring { get; init; }

    public string WgGunIndex { get; init; } = string.Empty;

    public decimal BaseAngle { get; init; }

    public decimal AmmoSwitchCoeff { get; init; }

    public Dispersion Dispersion { get; init; } = default!;
}
