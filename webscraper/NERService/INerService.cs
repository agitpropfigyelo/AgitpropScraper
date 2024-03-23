namespace webscraper;

public interface INerService
{
    Task<NerResponse> AnalyzeSingle(Article articleIn);
    Task<List<Article>> AnalyzeBatch(List<Article> articles);
}
