using GameParamsExtractor.WGStructure;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace WowsShipBuilder.GameParamsExtractor.Data;

public class GameParamsContainer : Dictionary<string, GameParamsCategory>
{
}

public class GameParamsCategory : Dictionary<string, List<WgObject>>
{
}
