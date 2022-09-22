using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

//these are populated when the skill has a condition to trigger.
//modifiers are the modifiers applied when the condition is met.
//Condition defined by triggerType and triggerDescIds ?
public class WgSkillTrigger
{
    public int BurnCount { get; set; }

    public float ChangePriorityTargetPenalty { get; set; }

    public string ConsumableType { get; set; } = string.Empty;

    public float CoolingDelay { get; set; }

    public object[] CoolingInterpolator { get; set; } = Array.Empty<object>();

    public string DividerType { get; set; } = string.Empty;

    public float DividerValue { get; set; }

    public float Duration { get; set; }

    public float EnergyCoeff { get; set; }

    public int FloodCount { get; set; }

    public object[] HeatInterpolator { get; set; } = Array.Empty<object>();

    public Dictionary<string, JToken> Modifiers { get; set; } = new();

    public string TriggerDescIds { get; set; } = string.Empty;

    public string TriggerType { get; set; } = string.Empty;
}
