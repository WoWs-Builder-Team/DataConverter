#pragma warning disable CA1716
namespace WoWsShipBuilder.DataStructures.Module;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class Module
#pragma warning restore CA1716
{
    public int CostCr { get; set; }

    public int CostXp { get; set; }

    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ModuleType Type { get; set; }
}
