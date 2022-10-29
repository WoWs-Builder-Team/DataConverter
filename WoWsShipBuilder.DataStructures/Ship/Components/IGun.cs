using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures.Ship.Components;

public interface IGun : IModuleBase
{
    public List<string> AmmoList { get; init; }

    public decimal BarrelDiameter { get; init; }

    public decimal[] HorizontalSector { get; init; }

    public decimal[][] HorizontalDeadZones { get; init; }

    public int NumBarrels { get; init; }

    public decimal HorizontalPosition { get; init; }

    public decimal VerticalPosition { get; init; }

    public decimal HorizontalRotationSpeed { get; init; }

    public decimal Reload { get; init; }

    public string WgGunIndex { get; set; }

    public decimal BaseAngle { get; set; }
}
