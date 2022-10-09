using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Captain;

public class UniqueSkill
{
    public Dictionary<string, UniqueSkillEffect> SkillEffects { get; set; } = new(); // dictionary of the effects and their names

    public int MaxTriggerNum { get; set; }

    public List<ShipClass> AllowedShips { get; set; } = new();

    public string TriggerType { get; set; } = string.Empty;

    public string TranslationId { get; set; } = string.Empty;
}
