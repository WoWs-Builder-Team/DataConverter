using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures.Components;

public interface IGun : IModuleBase
{
    public List<string> AmmoList { get; set; }
    public decimal BarrelDiameter { get; set; }
    public decimal[] HorizontalSector { get; set; }
    public decimal[][] HorizontalDeadZones { get; set; }
    public int NumBarrels { get; set; }
    public decimal HorizontalPosition { get; set; }
    public decimal VerticalPosition { get; set; }
    public decimal HorizontalRotationSpeed { get; set; }
    public decimal Reload { get; set; }
    public string WgGunIndex { get; set; }
    public decimal BaseAngle { get; set; }
}
