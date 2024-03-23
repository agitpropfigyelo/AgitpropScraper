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

    private async Task<Article> GetCorpus(Article articleIn)
    {
        TaskCompletionSource<Article> tcs = new();
        HtmlDocument doc = await GetHtml(articleIn);
        foreach (Func<HtmlDocument, string> scraper in scraperFunctions)
        {
            string result = scraper(doc);
            if (result.Length != 0)
            {
                articleIn.Corpus = result;
                return articleIn;
            }
        }
        throw new EmptyCorpusException("Not able to scrape site");
    }

    public async Task<List<Article>> GetCorpus(List<Article> articleIn, IProgress<int>? progress, CancellationToken? cancellationToken)
    {
        TaskCompletionSource tcs = new();
        List<Article> result = [];
        foreach (var article in articleIn)
        {
            try
            {
                Article res = await GetCorpus(article);
                result.Add(article);
                //System.Console.WriteLine("Add");
            }
            catch (System.Exception)
            {
                //System.Console.WriteLine("Fail");
            }
            progress?.Report(1);
        }
        return result;
    }

    public async Task<HtmlDocument> GetHtml(Article articleIn)
    {
        HtmlDocument result;
        HtmlWeb web = new HtmlWeb();
        web.OverrideEncoding = Encoding.UTF8;
        result = await web.LoadFromWebAsync(articleIn.Url.ToString());
        return result;
    }
}
