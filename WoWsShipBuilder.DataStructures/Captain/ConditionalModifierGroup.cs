using System.Collections.Immutable;

namespace WoWsShipBuilder.DataStructures.Captain;

public record ConditionalModifierGroup(string TriggerType, string TriggerDescription, ImmutableDictionary<string, float> Modifiers, int ActivationLimit = -1, string? LocalizationOverride = null);
