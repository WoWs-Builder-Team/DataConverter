namespace WoWsShipBuilder.DataStructures.Aircraft;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public sealed class PlaneAttackData
{
    public double AttackCooldown { get; init; }

    public int AttackCount { get; init; }

    public double AttackInterval { get; init; }

    public double AttackSpeedMultiplier { get; init; }

    public double AttackSpeedMultiplierApplyTime { get; init; }

    public double AttackerDamageTakenMultiplier { get; init; }

    public int AttackerSize { get; init; }
}
