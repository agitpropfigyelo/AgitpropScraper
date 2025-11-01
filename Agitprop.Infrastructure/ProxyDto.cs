using System.Text.Json.Serialization;

namespace Agitprop.Infrastructure;
public class ProxyDto
{
    [JsonPropertyName("shown_records")]
    public int ShownRecords { get; set; }

    [JsonPropertyName("total_records")]
    public int TotalRecords { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("skip")]
    public int Skip { get; set; }

    [JsonPropertyName("nextpage")]
    public bool NextPage { get; set; }

    [JsonPropertyName("proxies")]
    public List<Proxy> Proxies { get; set; } = new List<Proxy>();
}

public class Proxy
{
    [JsonPropertyName("alive")]
    public bool Alive { get; set; }

    [JsonPropertyName("alive_since")]
    public double AliveSince { get; set; }

    [JsonPropertyName("anonymity")]
    public string Anonymity { get; set; }

    [JsonPropertyName("average_timeout")]
    public double AverageTimeout { get; set; }

    [JsonPropertyName("first_seen")]
    public double FirstSeen { get; set; }

    [JsonPropertyName("ip_data")]
    public IpData IpData { get; set; }

    [JsonPropertyName("ip_data_last_update")]
    public long IpDataLastUpdate { get; set; }

    [JsonPropertyName("last_seen")]
    public double LastSeen { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; }

    [JsonPropertyName("proxy")]
    public string ProxyUrl { get; set; }

    [JsonPropertyName("ssl")]
    public bool Ssl { get; set; }

    [JsonPropertyName("timeout")]
    public double Timeout { get; set; }

    [JsonPropertyName("times_alive")]
    public int TimesAlive { get; set; }

    [JsonPropertyName("times_dead")]
    public int TimesDead { get; set; }

    [JsonPropertyName("uptime")]
    public double Uptime { get; set; }

    [JsonPropertyName("ip")]
    public string Ip { get; set; }
}

public class IpData
{
    [JsonPropertyName("as")]
    public string As { get; set; }

    [JsonPropertyName("asname")]
    public string AsName { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("continent")]
    public string Continent { get; set; }

    [JsonPropertyName("continentCode")]
    public string ContinentCode { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; }

    [JsonPropertyName("district")]
    public string District { get; set; }

    [JsonPropertyName("hosting")]
    public bool Hosting { get; set; }

    [JsonPropertyName("isp")]
    public string Isp { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("mobile")]
    public bool Mobile { get; set; }

    [JsonPropertyName("org")]
    public string Org { get; set; }

    [JsonPropertyName("proxy")]
    public bool Proxy { get; set; }

    [JsonPropertyName("regionName")]
    public string RegionName { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; }

    [JsonPropertyName("zip")]
    public string Zip { get; set; }
}
