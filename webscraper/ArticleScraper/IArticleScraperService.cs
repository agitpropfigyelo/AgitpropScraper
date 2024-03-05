using HtmlAgilityPack;

namespace webscraper;

public interface IArticleScraperService
{
    string GetCorpus(Article articleIn);
    HtmlDocument GetHtml(Article articleIn);
}
