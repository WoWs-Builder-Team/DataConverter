using WoWsShipBuilder.DataStructures.Common;

namespace WoWsShipBuilder.DataStructures
{
    public sealed record Module
    {
        public int CostCr { get; init; }
        public int CostXp { get; init; }
        public long Id { get; init; }
        public string Index { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public ModuleType Type { get; set; }
    }
}
