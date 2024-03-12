using Azure.AI.TextAnalytics;

namespace webscraper;

public interface INerService
{
    Task<NerResponse> AnalyzeSingle(Article articleIn);
    Task<List<NerResponse>> AnalyzeBatch(List<Article> articles);
}
