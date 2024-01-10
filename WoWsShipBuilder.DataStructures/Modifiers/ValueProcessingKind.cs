using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace WoWsShipBuilder.DataStructures.Modifiers;

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValueProcessingKind
{
    /// <summary>
    /// Processing kind has not been assigned.
    /// </summary>
    NotAssigned,

    /// <summary>
    /// Multiplies the base value by the modifier.
    /// </summary>
    Multiplier,

    /// <summary>
    /// Adds to the base value ((decimal)modifier * 100)
    /// </summary>
    AddPercentage,

    /// <summary>
    /// Subtract to the base value ((decimal)modifier / 100)
    /// </summary>
    SubtractPercentage,

    /// <summary>
    /// Multiplies the base value by (1 + ((decimal)modifier / 100)).
    /// </summary>
    PositiveMultiplier,

    /// <summary>
    /// Multiplies the base value by (1 - (mModifier / 100))
    /// </summary>
    NegativeMultiplier,

    /// <summary>
    /// Adds to the base value the modifier directly.
    /// </summary>
    RawAdd,

    /// <summary>
    /// Do nothing, value is not technically a modifier.
    /// </summary>
    None,
}
