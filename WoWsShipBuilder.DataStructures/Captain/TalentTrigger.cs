using System.Collections.Immutable;

namespace WoWsShipBuilder.DataStructures.Captain;

/// <summary>
/// Describes what makes a talent fire.
/// </summary>
/// <param name="ActivatorType">
/// The game's activator class name, for example RibbonActivator, DamageDoneActivator or RemainingHealthActivator.
/// It determines which key in <paramref name="Parameters"/> and in each level's thresholds is meaningful.
/// </param>
/// <param name="Parameters">Numeric activator settings, such as requiredCount or thresholdPerMaxHealth.</param>
/// <param name="MaxActivations">
/// The activator's own repeat limit, where -1 means the activator itself imposes none. This is NOT the number of
/// times the talent can fire: 17 of the 48 activators in build 13015811 report -1 while the talent is capped by
/// <see cref="UniqueSkill.MaxTriggerNum"/>. Use that for the talent's cap.
/// </param>
/// <param name="Levels">
/// Escalation thresholds for a tiered talent, ordered by level. Empty for a talent that does not escalate.
/// </param>
public sealed record TalentTrigger(
    string ActivatorType,
    ImmutableDictionary<string, decimal> Parameters,
    int MaxActivations,
    ImmutableList<TalentTriggerLevel> Levels);

/// <summary>
/// One escalation step of a tiered talent's trigger.
/// </summary>
/// <param name="Level">1-based tier number, matching <see cref="UniqueSkillEffectLevel.Level"/>.</param>
/// <param name="Thresholds">
/// What must be reached for this tier. The meaningful key depends on the activator: RibbonActivator uses
/// requiredCount, DamageDoneActivator uses damageIncrement, RemainingHealthActivator uses thresholdPerMaxHealth.
/// </param>
public sealed record TalentTriggerLevel(int Level, ImmutableDictionary<string, decimal> Thresholds);
