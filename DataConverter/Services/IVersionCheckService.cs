using System.Threading.Tasks;
using DataConverter.Data;
using WoWsShipBuilder.DataStructures.Versioning;

namespace DataConverter.Services;

/// <summary>
/// An interface defining a service that is used to compare results of a data conversion with existing data in order to create a version info.
/// </summary>
public interface IVersionCheckService
{
    /// <summary>
    /// Compares the results of a data conversion with the data from an existing online version info file.
    /// When no version info is found at the provided cdn host it will assume this is the first version and create a new version info.
    /// <br/>
    /// Files are compared based on their generated checksums, eliminating the need to download old files for comparison.
    /// </summary>
    /// <param name="conversionResult">The results of a data conversion.</param>
    /// <param name="gameVersion">The current game version object.</param>
    /// <param name="cdnHost">The host where an online version info can be found. The version info is retrieved from 'https://cdnHost/api/serverType/VersionInfo.json'
    /// (example: https://cdn.wowssb.com/api/live/VersionInfo.json).</param>
    /// <returns>The <see cref="VersionInfo"/> object created by comparing the converted data with the existing version info.</returns>
    Task<VersionInfo> CheckFileVersionsAsync(DataConversionResult conversionResult, GameVersion gameVersion, string cdnHost);

    /// <summary>
    /// Writes the content of a <see cref="VersionInfo"/> to a file.
    /// <br/>
    /// The file will be stored in the provided directory with the name 'VersionInfo.json'.
    /// </summary>
    /// <param name="versionInfo">The VersionInfo object to write.</param>
    /// <param name="outputBasePath">The directory where the file should be stored.</param>
    /// <returns>A task running the operation.</returns>
    public Task WriteVersionInfo(VersionInfo versionInfo, string outputBasePath);
}
