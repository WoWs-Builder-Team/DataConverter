using System.Collections.Generic;
using System.Threading.Tasks;
using DataConverter.Data;
using GameParamsExtractor.WGStructure;

namespace DataConverter.Services;

public interface IDataConverterService
{
    Task<DataConversionResult> ConvertRefinedData(Dictionary<string, Dictionary<string, List<WGObject>>> refinedData);

    public Task WriteConvertedData(DataConversionResult convertedData, string outputBasePath);
}
