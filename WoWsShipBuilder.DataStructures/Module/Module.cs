#pragma warning disable CA1716
namespace WoWsShipBuilder.DataStructures.Module;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class Module
#pragma warning restore CA1716
{
    public int CostCr { get; init; }

    public int CostXp { get; init; }

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public ModuleType Type { get; init; }
}
