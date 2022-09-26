namespace WoWsShipBuilder.DataStructures.Aircraft;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class PlaneAttackData
{
    public double AttackCooldown { get; set; }

    public int AttackCount { get; set; }

    public double AttackInterval { get; set; }

    public double AttackSpeedMultiplier { get; set; }

    public double AttackSpeedMultiplierApplyTime { get; set; }

    public double AttackerDamageTakenMultiplier { get; set; }

    public int AttackerSize { get; set; }
}
