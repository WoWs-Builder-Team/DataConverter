using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

//these are populated when the skill has a condition to trigger.
//modifiers are the modifiers applied when the condition is met.
//Condition defined by triggerType and triggerDescIds ?
public class WgSkillTrigger
{
    public int BurnCount { get; init; }

    public float ChangePriorityTargetPenalty { get; init; }

    public string ConsumableType { get; init; } = string.Empty;

    public float CoolingDelay { get; init; }

    public object[] CoolingInterpolator { get; init; } = Array.Empty<object>();

    public string DividerType { get; init; } = string.Empty;

    public float DividerValue { get; init; }

    public float Duration { get; init; }

    public float EnergyCoeff { get; init; }

    public int FloodCount { get; init; }

    public object[] HeatInterpolator { get; init; } = Array.Empty<object>();

    public Dictionary<string, JToken> Modifiers { get; init; } = new();

    public string TriggerDescIds { get; init; } = string.Empty;

    public string TriggerType { get; init; } = string.Empty;

    [JsonExtensionData]
    public Dictionary<string, JToken> OtherData { get; init; } = new();
}
