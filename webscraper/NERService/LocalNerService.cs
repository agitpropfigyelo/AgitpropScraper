using System.Text;
using System.Text.Json;
namespace webscraper;

public class LocalNerService : INerService
{
    private readonly HttpClient _httpClient;
    public LocalNerService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/"),
        };

    }


    public async Task<NerResponse> GetNamedEntities(Article articleIn)
    {
        var requestJson = $"{{ \"corpus\": \"{articleIn.Corpus}\" }}";
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("analyzeSingle", content);
        response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<NerResponse>(responseContent);;
    }
}
