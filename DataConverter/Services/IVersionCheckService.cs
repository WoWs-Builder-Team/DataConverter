using System.Threading.Tasks;
using DataConverter.Data;
using WoWsShipBuilder.DataStructures;

namespace DataConverter.Services;

public interface IVersionCheckService
{
    Task<VersionInfo> CheckFileVersionsAsync(DataConversionResult conversionResult, GameVersion gameVersion, string cdnHost);

    public Task WriteVersionInfo(VersionInfo versionInfo, string outputBasePath);
}
