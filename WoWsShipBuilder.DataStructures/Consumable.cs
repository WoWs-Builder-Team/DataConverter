using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures;

public class Consumable
{
    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DescId { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public string IconId { get; set; } = string.Empty;

    public int NumConsumables { get; set; }

    public float ReloadTime { get; set; }

    public float WorkTime { get; set; }

    public string ConsumableVariantName { get; set; } = string.Empty;

    public string PlaneName { get; set; } = string.Empty;

    public Dictionary<string, float> Modifiers { get; set; } = new();
}
