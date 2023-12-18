using System.Collections.Generic;
using System.Threading.Tasks;
using DataConverter.Data;
using WoWsShipBuilder.DataStructures.Modifiers;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Services;

public interface IDataConverterService
{
    Task<DataConversionResult> ConvertRefinedData(Dictionary<string, Dictionary<string, List<WgObject>>> refinedData, bool writeModifierDebugOutput, Dictionary<string, Modifier> modifiersDictionary, Dictionary<long, int> techTreeShipsPositionsDictionary);

    public Task WriteConvertedData(DataConversionResult convertedData, string outputBasePath);
}
