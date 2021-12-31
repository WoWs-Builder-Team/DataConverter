using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures
{
    public sealed record Consumable
    {
        public long Id { get; init; }
        public string Index { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string DescId { get; init; } = string.Empty;
        public string Group { get; init; } = string.Empty;
        public string IconId { get; init; } = string.Empty;
        public int NumConsumables { get; init; }
        public float ReloadTime { get; init; }
        public float WorkTime { get; init; }
        public string ConsumableVariantName { get; init; } = string.Empty;
        public Dictionary<string, float> Modifiers { get; init; } = new();
    }
}
