namespace WoWsShipBuilder.DataStructures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class Module
{
    public int CostCr { get; set; }
    public int CostXp { get; set; }
    public long Id { get; set; }
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ModuleType Type { get; set; }
}
