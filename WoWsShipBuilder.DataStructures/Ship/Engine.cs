namespace WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public sealed class Engine
{
    public decimal BackwardEngineUpTime { get; init; }

    public decimal ForwardEngineUpTime { get; init; }

    public decimal BackwardEngineForsag { get; init; }

    public decimal BackwardEngineForsagMaxSpeed { get; init; }

    public decimal ForwardEngineForsag { get; init; }

    public decimal ForwardEngineForsagMaxSpeed { get; init; }

    public decimal SpeedCoef { get; init; }

    public decimal ArmorCoeff { get; init; }
}
