// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgEngine : WgArmamentModule
{
    public decimal BackwardEngineUpTime { get; init; }

    public decimal ForwardEngineUpTime { get; init; }

    public decimal BackwardEngineForsag { get; init; }

    public decimal BackwardEngineForsagMaxSpeed { get; init; }

    public decimal ForwardEngineForsag { get; init; }

    public decimal ForwardEngineForsagMaxSpeed { get; init; }

    public decimal SpeedCoef { get; init; }

    public HitLocationEngine HitLocationEngine { get; init; } = new();
}

public class HitLocationEngine
{
    public decimal ArmorCoeff { get; init; }
}
