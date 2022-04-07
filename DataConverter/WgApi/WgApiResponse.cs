using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DataConverter.WgApi;

public record WgApiResponse(string Status, Meta Meta, Dictionary<string, ShipImage> Data, Error Error);

public record Meta(int Count, [property: JsonPropertyName("page_total")] int PageTotal, int Total, int Limit, int Page);

public record ShipImage([property: JsonPropertyName("ship_id_str")] string ShipIdStr, WgImage Images);

public record WgImage(string Large, string Small, string Medium);

public record Error(string Field, string Message, int Code, string Value);
