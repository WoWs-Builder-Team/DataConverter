using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace WoWsShipBuilder.DataStructures.Modifiers;

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DisplayValueProcessingKind
{
    /// <summary>
    /// Processing kind has not been assigned.
    /// </summary>
    NotAssigned,

    /// <summary>
    /// Value as is.
    /// </summary>
    Raw,

    /// <summary>
    /// Adds a - in front of the value.
    /// </summary>
    ToNegative,

    /// <summary>
    /// Adds a + in front of the value.
    /// </summary>
    ToPositive,

    /// <summary>
    /// Value is discarded and an empty string is returned.
    /// </summary>
    Discard,

    /// <summary>
    /// Value should be converted to an int. adding a + if value is positive.
    /// </summary>
    ToInt,

    /// <summary>
    /// If value is > 1 then it's to be left as is, otherwise it follows the formula {Math.Round(modifier * 100, 2)}, adding a + in front.
    /// </summary>
    /// <remarks>Value has to be positive.</remarks>
    /// <example>0.5 -> +50<br/>1.5 -> +1.5</example>
    RawOrPercentage,

    /// <summary>
    /// Value is always a positive percentage, that follows the formula: +{(modifier - 1) * 100}.
    /// </summary>
    /// <example>1.05 -> +5</example>
    PositivePercentage,

    /// <summary>
    /// Value is always a negative percentage, that follows the formula: -{(int)Math.Round(modifier * 100)}.
    /// </summary>
    /// <example>0.95 -> -95</example>
    NegativePercentage,

    /// <summary>
    /// Value is a percentage that should be shown as negative buff. Follows the formula: -(Math.Round((1 - modifier) * 100)).
    /// </summary>
    /// <example>0.95 -> -5</example>
    InverseNegativePercentage,

    /// <summary>
    /// Value is a percentage, sign depends on the value itself. Follows the formula: (Math.Round(modifier * 100, 2) - 100), adding a + in front if value is positive.
    /// </summary>
    /// <example>1.05 -> +5<br/>0.95 -> -5</example>
    VariablePercentage,

    /// <summary>
    /// Value is a percentage, converted to int. Follows the formula: (int)(Math.Round(modifier * 100, 2) - 100), adding a + in front if value is positive.
    /// </summary>
    /// <example>1.055 -> +5<br/>0.955 -> -5</example>
    IntVariablePercentage,

    /// <summary>
    /// Value is a percentage, sign depends on the value itself. Follows the formula: {Math.Round(modifier * 100)}, adding a + if necessary.
    /// </summary>
    /// <example>0.055 -> +6<br/>-0.055 -> -6</example>
    RoundedPercentage,

    /// <summary>
    /// Value is a percentage, sign depends on the value itself. Follows the formula {Math.Round((modifier - 1) * 100, 2)} if value > 1, {Math.Round((1 - modifier) * 100, 2)} otherwise.
    /// Adds a + in front if value is > 1, adds a - in front otherwise.
    /// </summary>
    /// <example>1.055 -> +5.5<br/>0.945 -> -5.5</example>
    DecimalRoundedPercentage,

    /// <summary>
    /// Value is in Big World unit and should be transformed in km. Formula is: {(modifier * 30) / 1000}.
    /// </summary>
    /// <remarks>1 BW = 30 m</remarks>
    /// <example>10 -> 0.3</example>
    BigWorldToKm,

    /// <summary>
    /// Value is in Big World unit and should be transformed in km, with added decimal. Formula is: {(modifier * 30) / 1000:.##}
    /// </summary>
    /// <remarks>1 BW = 30 m</remarks>
    /// <example>10 -> 0.3</example>
    BigWorldToKmDecimal,

    /// <summary>
    /// Value is in meter and should be converted to km. Formula is: {modifier / 1000}
    /// </summary>
    MeterToKm,
}
