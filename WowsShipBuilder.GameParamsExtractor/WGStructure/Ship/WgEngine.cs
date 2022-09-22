// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgEngine : WgArmamentModule
{
    public decimal BackwardEngineUpTime { get; set; }

    public decimal ForwardEngineUpTime { get; set; }

    public decimal BackwardEngineForsag { get; set; }

    public decimal BackwardEngineForsagMaxSpeed { get; set; }

    public decimal ForwardEngineForsag { get; set; }

    public decimal ForwardEngineForsagMaxSpeed { get; set; }

    public decimal SpeedCoef { get; set; }

    public HitLocationEngine HitLocationEngine { get; set; } = new();
}

public class HitLocationEngine
{
    public decimal ArmorCoeff { get; set; }
}
