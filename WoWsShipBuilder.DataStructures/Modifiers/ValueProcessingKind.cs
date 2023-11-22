using System.Text.Json.Serialization;

namespace WoWsShipBuilder.DataStructures.Modifiers;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValueProcessingKind
{
    /**
     * Processing kind has not been assigned
     */
    NotAssigned,
    /**
     * Multiplies the base value by the modifier.
     */
    Multiplier,
    /**
     * Adds to the base value ((decimal)modifier * 100)
     */
    SumPercentage,
    /**
    * Subtract to the base value ((decimal)modifier / 100)
    */
    SubtractPercentage,
    /**
     * Multiplies the base value by (1 + ((decimal)modifier / 100)).
     */
    PositiveMultiplier,
    /**
     * Adds to the base value the modifier directly.
     */
    DirectAdd,
    /**
     * Do nothing, value is not tecnically a modifier.
     */
    None,
}
