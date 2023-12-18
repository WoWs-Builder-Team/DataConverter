using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Captain;

public sealed class Skill
{
    public bool CanBeLearned { get; init; }

    public bool IsEpic { get; init; } // true if the skill has buffed modifier

    public int SkillNumber { get; init; }

    public ImmutableArray<ShipClass> LearnableOn { get; init; } = ImmutableArray<ShipClass>.Empty;

    public ImmutableArray<SkillPosition> Tiers { get; init; } = ImmutableArray<SkillPosition>.Empty; // contains the tier of the skill for all the classes that can use it

    public ImmutableList<Modifier> Modifiers { get; init; } = ImmutableList<Modifier>.Empty;

    public ImmutableArray<ConditionalModifierGroup> ConditionalModifierGroups { get; init; } = ImmutableArray<ConditionalModifierGroup>.Empty;
}
