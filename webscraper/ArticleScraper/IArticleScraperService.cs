using HtmlAgilityPack;

namespace webscraper;

public interface IArticleScraperService
{
    Task<List<Article>> GetCorpus(List<Article> articleIn, IProgress<int>? progress = null, CancellationToken? cancellationToken = null);
    Task<HtmlDocument> GetHtml(Article articleIn);
}
