
using Azure;
using Azure.AI.TextAnalytics;

namespace webscraper;

public class AzureNerService : INerService
{
    private static readonly AzureKeyCredential credentials = new AzureKeyCredential("2b5adce9688b46e580cc7af9738be241");
    private static readonly Uri endpoint = new Uri("https://agitpropfigyelo.cognitiveservices.azure.com/");
    private TextAnalyticsClient client;
    public AzureNerService()
    {
        this.client = new TextAnalyticsClient(endpoint, credentials);
    }
    public List<CategorizedEntity> GetNamedEntities(Article articleIn)
    {
        Response<CategorizedEntityCollection> response = client.RecognizeEntities(articleIn.Corpus);
        return response.Value.ToList();
    }

    Dictionary<string, List<string>> INerService.GetNamedEntities(Article articleIn)
    {
        throw new NotImplementedException();
    }
}
