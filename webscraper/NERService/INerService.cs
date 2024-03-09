using Azure.AI.TextAnalytics;

namespace webscraper;

public interface INerService
{
    Task<NerResponse> GetNamedEntities(Article articleIn);
}
