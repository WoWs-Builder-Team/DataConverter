using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace WoWsShipBuilder.DataStructures.Captain;

// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record UniqueSkillEffect(bool IsPercent, int UniqueType, ImmutableList<Modifier> Modifiers)
{
    /// <summary>
    /// Gets the escalation steps of a tiered talent, ordered by level. Empty for a talent that does not escalate.
    /// Declared as an init property rather than a positional parameter so that adding it does not break consumers
    /// that construct or deconstruct this record.
    /// </summary>
    /// <remarks>
    /// When this is non-empty, <see cref="Modifiers"/> holds the cumulative values of the highest tier, so a consumer
    /// that ignores tiers still shows a fully-escalated talent rather than nothing.
    /// </remarks>
    public ImmutableList<UniqueSkillEffectLevel> Levels { get; init; } = ImmutableList<UniqueSkillEffectLevel>.Empty;
}
