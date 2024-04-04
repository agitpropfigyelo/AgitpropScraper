using System.Xml;
using HtmlAgilityPack;
using Louw.SitemapParser;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class HuszonnegyScraper : INewsSiteScraper
{
    private readonly Uri baseUri = new Uri("https://www.24.hu");
    private readonly Uri freshSiteMap = new Uri("https://24.hu/app/uploads/sitemap/24.hu_sitemap_fresh.xml");

    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        HtmlNode titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='o-post__title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var leadNode = document.DocumentNode.SelectSingleNode("//h1[@class='o-post__lead lead post-lead cf _ce_measure_widget']");
        string leadText = leadNode is not null ? leadNode.InnerText.Trim() + " " : "";

        // Select nodes with class "article-lead"
        var articleNode = document.DocumentNode.SelectSingleNode("//div[@class='o-post__body post-body']");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + leadText + articleText;


        return Helper.CleanUpText(concatenatedText);

    }

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        throw new NotImplementedException();
    }
}
