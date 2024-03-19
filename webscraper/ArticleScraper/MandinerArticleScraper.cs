using HtmlAgilityPack;

namespace webscraper;

public class MandinerArticleScraper : IArticleScraperService
{
    public Task<List<Article>> GetCorpus(List<Article> articleIn, IProgress<Article>? progress = null, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public Task<HtmlDocument> GetHtml(Article articleIn)
    {
        throw new NotImplementedException();
    }
}
