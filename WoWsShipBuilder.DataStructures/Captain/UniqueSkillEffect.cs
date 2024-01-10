using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace WoWsShipBuilder.DataStructures.Captain;

// ReSharper disable NotAccessedPositionalProperty.Global
public sealed record UniqueSkillEffect(bool IsPercent, int UniqueType, ImmutableList<Modifier> Modifiers);
