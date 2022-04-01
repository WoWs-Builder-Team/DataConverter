using System.Collections.Generic;

namespace DataConverter.WgApi;

public class WgApiResponse
{
    public string status { get; set; }

    public Meta meta { get; set; }

    public Dictionary<string, ShipImage> data { get; set; }

    public Error error { get; set; }
}

public class Meta
{
    public int count { get; set; }

    public int page_total { get; set; }

    public int total { get; set; }

    public int limit { get; set; }

    public int page { get; set; }
}

public class ShipImage
{
    public string ship_id_str { get; set; }

    public WgImage images { get; set; }
}

public class WgImage
{
    public string large { get; set; }

    public string small { get; set; }
    public string medium { get; set; }
}

public class Error
{
    public string field { get; set; }

    public string message { get; set; }

    public int code { get; set; }

    public string value { get; set; }
}
