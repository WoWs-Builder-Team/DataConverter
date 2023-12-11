using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace WoWsShipBuilder.DataStructures.Captain;

public sealed record ConditionalModifierGroup(string TriggerType, string TriggerDescription, ImmutableList<Modifier> Modifiers, int ActivationLimit = -1, string? LocalizationOverride = null);
