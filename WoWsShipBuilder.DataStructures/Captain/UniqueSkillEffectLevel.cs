using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace WoWsShipBuilder.DataStructures.Captain;

/// <summary>
/// One escalation step of a tiered talent's effect.
/// </summary>
/// <param name="Level">1-based tier number, matching <see cref="TalentTriggerLevel.Level"/>.</param>
/// <param name="Modifiers">
/// The increment applied when this tier is reached. For multiplicative stats these compound across tiers, so this is
/// not the value the player sees at this tier.
/// </param>
/// <param name="CumulativeModifiers">
/// The effective values once this tier is active, as shown in game. For multiplicative stats the game ships these
/// separately (a "...UI" twin); for absolute stats there is no twin and the value equals the one in
/// <paramref name="Modifiers"/>.
/// </param>
public sealed record UniqueSkillEffectLevel(int Level, ImmutableList<Modifier> Modifiers, ImmutableList<Modifier> CumulativeModifiers);
