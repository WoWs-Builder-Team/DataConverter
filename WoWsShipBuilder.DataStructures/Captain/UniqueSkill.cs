using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Captain;

public sealed class UniqueSkill
{
    public ImmutableDictionary<string, UniqueSkillEffect> SkillEffects { get; init; } = ImmutableDictionary<string, UniqueSkillEffect>.Empty; // dictionary of the effects and their names

    public int MaxTriggerNum { get; init; }

    public ImmutableArray<ShipClass> AllowedShips { get; init; } = ImmutableArray<ShipClass>.Empty;

    public string TriggerType { get; init; } = string.Empty;

    public string TranslationId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the battle types this talent variant applies to. Talents tuned differently for operations ship as two
    /// entries sharing a <see cref="TranslationId"/>, one Regular and one Operations; consumers that only model
    /// random battles should skip <see cref="TalentBattleGroup.Operations"/> to avoid listing a talent twice.
    /// </summary>
    public TalentBattleGroup BattleGroup { get; init; } = TalentBattleGroup.Every;

    /// <summary>
    /// Gets what makes this talent fire, or null if the game data carried no trigger definition.
    /// </summary>
    public TalentTrigger? Trigger { get; init; }
}
