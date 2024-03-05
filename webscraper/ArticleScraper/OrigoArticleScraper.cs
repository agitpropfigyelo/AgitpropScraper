using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using HtmlAgilityPack;

namespace webscraper;

public class OrigoArticleScraper : IArticleScraperService
{
    readonly List<Func<HtmlDocument, string>> scraperFunctions = [
            (doc)=>{
                IEnumerable<string> idk=doc.DocumentNode.Descendants("section")
                                .Where(section => section.GetAttributeValue("class", "") == "article")
                                .SelectMany(section => section.Descendants("p").Select(p => p.InnerText.Trim()));
                return string.Join(' ',idk).Replace("&nbsp;","");
            },
    ];

    public string GetCorpus(Article articleIn)
    {
        HtmlDocument doc = GetHtml(articleIn);
        foreach (Func<HtmlDocument, string> scraper in scraperFunctions)
        {
            string result = scraper(doc);
            if (result.Length != 0)
            {
                return result;
            }
        }
        throw new EmptyCorpusException("Not able to scrape site");
    }

    public HtmlDocument GetHtml(Article articleIn)
    {
        HtmlDocument result;
        HtmlWeb web = new HtmlWeb();
        web.OverrideEncoding = Encoding.UTF8;
        Stopwatch stopwatch = new();

        stopwatch.Start();
        result = web.LoadFromWebAsync(articleIn.Url.ToString()).Result;
        stopwatch.Stop();

        return result;
    }
}
