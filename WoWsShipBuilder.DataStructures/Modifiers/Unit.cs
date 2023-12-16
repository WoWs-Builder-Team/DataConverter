using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace WoWsShipBuilder.DataStructures.Modifiers;

[PublicAPI]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Unit
{
    NotAssigned,
    None,
    Kilograms,
    Millimeters,
    Meters,
    Kilometers,
    Seconds,
    Degrees,
    Percent,
    Hitpoints,
    Knots,
    MetersPerSecond,
    DegreesPerSecond,
    PercentPerSecond,
    DamagePerSecond,
    ShotsPerMinute,
    FiresPerMinute,
}
