﻿using Newtonsoft.Json;
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

    [JsonExtensionData]
    public Dictionary<string, JToken> SkillEffects { get; init; } = new(); // value is actually Dictionary<string, object>, process in converter
}
