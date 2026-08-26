using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

public class WgUniqueSkill
{
    public int MaxTriggerNum { get; init; }

    public ShipClass[] TriggerAllowedShips { get; init; } = Array.Empty<ShipClass>();

    public string TriggerType { get; init; } = string.Empty;

    /// <summary>
    /// Which battle types this talent variant applies to (BATTLE_GROUP_EVERY, _REGULAR or _OPERATIONS).
    /// Declared explicitly so it does not land in <see cref="SkillEffects"/> and get scanned for modifiers.
    /// </summary>
    public string BattleGroup { get; init; } = string.Empty;

    /// <summary>
    /// Position of this talent within its captain's talent list. Part of the localization key, and shared by the
    /// Regular and Operations copies of the same talent. Declared explicitly for the same reason as
    /// <see cref="BattleGroup"/>.
    /// </summary>
    public int SortIndex { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JToken> SkillEffects { get; init; } = new(); // value is actually Dictionary<string, object>, process in converter
}
