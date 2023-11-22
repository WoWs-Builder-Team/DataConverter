using System.Text.Json.Serialization;

namespace WoWsShipBuilder.DataStructures.Modifiers;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DisplayValueProcessingKind
{
    /**
     * Processing kind has not been assigned
     */
    NotAssigned,
    /**
     * Value as is.
     */
    None,
    /**
     * Adds a - in front of the value.
     */
    ToNegative,
    /**
     * Adds a + in front of the value.
     */
    ToPositive,
    /**
     * Value is discarded and an empty string is returned.
     */
    Empty,
    /**
     * Value should be converted to an int. adding a + if value is positive.
     */
    ToInt,
    /**
     * Value is always > 0 to be left as is or converted to a percentage. If value is > 1 then it's to be left as is, otherwise it follows the formula {Math.Round(modifier * 100, 2)}, adding a + in front
     */
    NoneOrPercentage,
    /**
     * Value is always a positive percentage, that follows the formula: +{(modifier - 1) * 100}.
     */
    PositivePercentage,
    /**
     * Value is always a negative percentage, that follows the formula: -{(int)Math.Round(modifier * 100)}.
     */
    NegativePercentage,
    /**
     * Value is a percentage that should be shown as negative buff. Follows the formula: -(Math.Round((1 - modifier) * 100)).
     */
    InverseNegativePercentage,
    /**
     * Value is a percentage, sign depends on the value itself. Follows the formula: (Math.Round(modifier * 100, 2) - 100), adding a + in front if value is positive.
     */
    VariablePercentage,
    /**
     * Value is a percentage, converted to int. Follows the formula: (int)(Math.Round(modifier * 100, 2) - 100), adding a + in front if value is positive.
     */
    IntVariablePercentage,
    /**
     * Probably better name to be found. Used only by planeForsageDrainRate because WG can't math.
     * Follows the formula Math.Round(((1 / modifier) - 1) * 100, 2), adding a + in front if positive.
     */
    DrainPercentage,
    /**
     * Value is a percentage, sign depends on the value itself. Follows the formula {Math.Round((modifier - 1) * 100, 2)} if positive, {Math.Round((1 - modifier) * 100, 2)} if negative.
     * Adds a + in front if value is positive.
     */
    DecimalRoundedPercentage,
    /**
     * Value is a percentage, sign depends on the value itself. Follows the formula: {Math.Round(modifier * 100)}, adding a + if necessary.
     */
    RoundedPercentage,
    /**
     * Value is in Big World unit and should be transformed in km. Formula is: {(modifier * 30) / 1000}.
     */
    BigWorldToKm,
    /**
     * Value is in Big World unit and should be transformed in km, with added decimal. Formula is: {(modifier * 30) / 1000:.##}
     */
    BigWorldToKmDecimal,
    /**
     * Value is in meter and should be converted to km. Formula is: {modifier / 1000}
     */
    MeterToKm,
}
