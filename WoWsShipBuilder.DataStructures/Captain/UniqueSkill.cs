using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Captain;

public class UniqueSkill
{
    public ImmutableDictionary<string, UniqueSkillEffect> SkillEffects { get; init; } = ImmutableDictionary<string, UniqueSkillEffect>.Empty; // dictionary of the effects and their names

    public int MaxTriggerNum { get; init; }

    public ImmutableArray<ShipClass> AllowedShips { get; init; } = ImmutableArray<ShipClass>.Empty;

    public string TriggerType { get; init; } = string.Empty;

    public string TranslationId { get; init; } = string.Empty;
}
