using Azure.AI.TextAnalytics;

namespace webscraper;

public interface INerService
{
    Dictionary<string, List<string>> GetNamedEntities(Article articleIn);
}
