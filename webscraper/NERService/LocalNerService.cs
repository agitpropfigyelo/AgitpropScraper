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

    public async Task<List<NerResponse>> AnalyzeBatch(List<Article> articles)
    {
        try
        {
            string requestContent = JsonSerializer.Serialize(articles.Select(a => a.Corpus));
            StringContent content = new StringContent(requestContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("analyzeBatch", content);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();

            List<NerResponse> result = JsonSerializer.Deserialize<List<NerResponse>>(responseContent) ?? new List<NerResponse>();
            return result;
        }
        catch (System.Exception)
        {
            return new List<NerResponse>();
        }
    }

    public async Task<NerResponse> AnalyzeSingle(Article articleIn)
    {
        var requestJson = $"{{ \"corpus\": \"{articleIn.Corpus}\" }}";
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("analyzeSingle", content);
        response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonSerializer.Deserialize<NerResponse>(responseContent) ?? new NerResponse();
        }
        catch
        {
            return new NerResponse();
        }
    }
}
