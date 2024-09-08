using System.Globalization;
using System.Xml;
using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure;
using HtmlAgilityPack;

namespace Agitprop.Scrapers.Metropol;

public class ArchivePaginator : SitemapArchivePaginator, IPaginator
{
    public ScrapingJob GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new ScrapingJobBuilder().SetUrl(base.GetUrl(currentUrl, document))
                                       .SetPageType(PageType.Static)
                                       .SetPageCategory(PageCategory.PageWithPagination)
                                       .AddContentParser(new ArticleContentParser())
                                       .AddPagination(new ArchivePaginator())
                                       .Build();
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return Task.FromResult(GetNextPage(currentUrl, doc));
    }
}

public class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("/html/body/app-root/app-base/div/app-article-page/section/div[1]/div/metropol-article-header/section/div[5]/div[2]/div");
        DateTime date = DateTime.Parse(dateNode.InnerText.Split(':')[1]);

        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//h1[contains(@class, 'article-header-title')]");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var leadNode = html.DocumentNode.SelectSingleNode("//p[contains(@class, 'article-header-lead')]");
        string leadText = leadNode.InnerText.Trim() + " ";

        // Select nodes with tag "origo-wysiwyg-box"
        var boxNodes = html.DocumentNode.SelectNodes("//metropol-wysiwyg-box");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

        // Concatenate all text
        string concatenatedText = titleText + leadText + boxText;

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.Metropol,
            Text = Helper.CleanUpText(concatenatedText)
        });
    }

    public Task<ContentParserResult> ParseContentAsync(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return this.ParseContentAsync(doc);
    }
}

public class ArchiveLinkParser : SitemapLinkParser, ILinkParser
{
    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        var result = GetLinks(docString).Select(link => new ScrapingJobBuilder().SetUrl(link)
                                                                                .SetPageType(PageType.Static)
                                                                                .SetPageCategory(PageCategory.TargetPage)
                                                                                .Build()).ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var result = GetLinks(doc.ToString()).Select(link => new ScrapingJobBuilder().SetUrl(link)
                                                                                .SetPageType(PageType.Static)
                                                                                .SetPageCategory(PageCategory.TargetPage)
                                                                                .AddContentParser(new ArticleContentParser())
                                                                                .Build()).ToList();
        return Task.FromResult(result); ;
    }
}

public class MetropolScraper
{
    private readonly Uri baseUri = new Uri("https://www.metropol.hu");

    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectSingleNode("//h1[contains(@class, 'article-header-title')]");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var leadNode = document.DocumentNode.SelectSingleNode("//p[contains(@class, 'article-header-lead')]");
        string leadText = leadNode.InnerText.Trim() + " ";

        // Select nodes with tag "origo-wysiwyg-box"
        var boxNodes = document.DocumentNode.SelectNodes("//metropol-wysiwyg-box");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

        // Concatenate all text
        string concatenatedText = titleText + leadText + boxText;

        return Helper.CleanUpText(concatenatedText);
    }

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        var suffix = $"{dateIn:yyyyMM}_sitemap.xml";
        Uri weblink = new(baseUri!, suffix);
        string sitemapUrl = weblink.ToString();

        List<string> resultList = []; // Initialize the list

        using (HttpClient client = new HttpClient())
        {
            try
            {
                var response = await client.GetStringAsync(sitemapUrl);

                XmlDocument document = new XmlDocument();
                document.LoadXml(response);

                XmlNodeList urlNodes = document.GetElementsByTagName("url");

                foreach (XmlElement urlNode in urlNodes)
                {
                    XmlNodeList childNodes = urlNode.ChildNodes;
                    string location = childNodes[0]!.InnerText;
                    DateTime timestamp = DateTime.Parse(childNodes[1]!.InnerText, CultureInfo.InvariantCulture);
                    var offset = DateTimeOffset.Parse(childNodes[1]!.InnerText, CultureInfo.InvariantCulture);
                    if (offset.Date == dateIn.Date)
                    {
                        resultList.Add(location);
                    }
                }
            }
            catch (Exception ex)
            {
                // Rethrow the exception as a task result
                throw new InvalidOperationException("Error occurred while fetching articles", ex);
            }
        }

        return resultList; // Return the list as a task result
    }
}
