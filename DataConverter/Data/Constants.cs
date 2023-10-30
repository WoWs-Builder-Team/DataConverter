using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataConverter.Data;

public static class Constants
{
    public const string CdnHost = "cdn.wowssb.com";

    public const string ShiptoolDataUrl = "https://shiptool.st/api/data";

    public const float Tolerance = 0.001f;

    public const int BigWorld = 30; // 1 BW = 30 meters

    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };
}
