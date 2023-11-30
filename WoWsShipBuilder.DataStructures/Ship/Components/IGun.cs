using System.Collections.Immutable;

namespace WoWsShipBuilder.DataStructures.Ship.Components;

public interface IGun : IModuleBase
{
    public ImmutableArray<string> AmmoList { get; init; }

    public decimal BarrelDiameter { get; init; }

    public ImmutableArray<decimal> HorizontalSector { get; init; }

    public ImmutableArray<ImmutableArray<decimal>> HorizontalDeadZones { get; init; }

    public int NumBarrels { get; init; }

    public decimal HorizontalPosition { get; init; }

    public decimal VerticalPosition { get; init; }

    public decimal HorizontalRotationSpeed { get; init; }

    public decimal Reload { get; init; }

    public string WgGunIndex { get; init; }

    public decimal BaseAngle { get; init; }
}
