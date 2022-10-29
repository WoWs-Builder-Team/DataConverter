namespace WoWsShipBuilder.DataStructures.Ship.Components;

public interface IModuleBase
{
    public long Id { get; init; }

    public string Index { get; init; }

    public string Name { get; init; }
}
