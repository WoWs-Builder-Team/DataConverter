using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

//big skill structure. modifiers contain always on effects.
public class WgSkill
{
    public WgSkillTrigger LogicTrigger { get; init; } = new();

    public bool CanBeLearned { get; init; }

    public bool IsEpic { get; init; }

    public Dictionary<string, JToken> Modifiers { get; init; } = new(); // either a float or a ModifiersPerClass object

    //this is actually the skill number
    public int SkillType { get; init; }
}
