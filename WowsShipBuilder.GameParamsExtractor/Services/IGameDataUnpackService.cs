using WowsShipBuilder.GameParamsExtractor.Data;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace WowsShipBuilder.GameParamsExtractor.Services;

public interface IGameDataUnpackService
{
    /// <summary>
    /// Extracts data from a game params file, refines it and returns the result.
    /// </summary>
    /// <param name="options">The <see cref="GameParamsExtractionOptions"/> that configure the extraction process.</param>
    /// <returns>A dictionary containing an entry for each type category from the game params.
    /// Each entry consists of another dictionary mapping nations to the list of their objects.</returns>
    GameParamsExtractionResult ExtractAndRefineGameParams(GameParamsExtractionOptions options);

    /// <summary>
    /// Extracts data from a game params file and returns the raw result.
    /// <br/>
    /// <b>Only use this method if you know what you do and using the <see cref="ExtractAndRefineGameParams"/> method is not possible.</b>
    /// </summary>
    /// <param name="gameParamsFilePath">The file path of the game params file.</param>
    /// <returns>A dictionary containing the raw, unrefined data from the game params file.</returns>
    Dictionary<object, Dictionary<string, object>> ExtractRawGameParamsData(string gameParamsFilePath);

    void WriteUnfilteredFiles(Dictionary<string, Dictionary<string, List<SortedDictionary<string, object>>>> rawGameParams, string outputBasePath);

    void WriteFilteredFiles(Dictionary<string, Dictionary<string, List<WgObject>>> refinedGameParams, string outputBasePath);
}
