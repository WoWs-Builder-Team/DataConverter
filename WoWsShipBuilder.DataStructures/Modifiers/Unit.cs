using System.Text.Json.Serialization;

namespace WoWsShipBuilder.DataStructures.Modifiers;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Unit
{
    NotAssigned,
    None,
    MPS,
    KG,
    MM,
    Percent,
    Degree,
    S,
    PercentPerS,
    KM,
    DegreePerSecond,
    DPS,
    HP,
    Knots,
    M,
    ShotsPerMinute,
    FPM,
}
