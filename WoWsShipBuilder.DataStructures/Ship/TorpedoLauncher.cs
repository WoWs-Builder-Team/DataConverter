using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataStructures.Components;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class TorpedoLauncher : IGun
{
    public List<string> AmmoList { get; init; } = new();

    public decimal BarrelDiameter { get; init; }

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public decimal[][] HorizontalDeadZones { get; init; } = Array.Empty<decimal[]>();

    public int NumBarrels { get; init; }

    public decimal HorizontalPosition { get; init; }

    public decimal VerticalPosition { get; init; }

    public decimal HorizontalRotationSpeed { get; init; }

    public decimal VerticalRotationSpeed { get; set; }

    public decimal Reload { get; init; }

    public string WgGunIndex { get; set; } = string.Empty;

    public decimal[] HorizontalSector { get; init; } = Array.Empty<decimal>();

    public decimal[] TorpedoAngles { get; set; } = Array.Empty<decimal>();

    public decimal BaseAngle { get; set; }
}
