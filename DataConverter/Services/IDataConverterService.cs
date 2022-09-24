using System.Collections.Generic;
using System.Threading.Tasks;
using DataConverter.Data;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Services;

public interface IDataConverterService
{
    Task<DataConversionResult> ConvertRefinedData(Dictionary<string, Dictionary<string, List<WgObject>>> refinedData);

    public Task WriteConvertedData(DataConversionResult convertedData, string outputBasePath);
}
