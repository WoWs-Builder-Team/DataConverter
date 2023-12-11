using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Captain;

public sealed class Captain
{
    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool HasSpecialSkills { get; init; }

    public ImmutableDictionary<string, Skill> Skills { get; init; } = ImmutableDictionary<string, Skill>.Empty;

    public ImmutableDictionary<string, UniqueSkill> UniqueSkills { get; init; } = ImmutableDictionary<string, UniqueSkill>.Empty;

    public Nation Nation { get; init; }
}
