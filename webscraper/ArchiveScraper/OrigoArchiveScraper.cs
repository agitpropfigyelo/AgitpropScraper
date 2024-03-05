using System.Text;
using HtmlAgilityPack;

namespace webscraper
{
    public class OrigoArchiveScraper : IArchiveScraperService
    {
        private readonly Uri baseUri = new Uri("https://www.origo.hu");

        public Task<IEnumerable<Article>> GetArticlesForDayAsync(DateTime date)
        {
            List<Article> resultArticles = [];
            string endOfUri = $"/hir-archivum/{date.Year}/{date:yyyyMMdd}.html";
            Uri Url = new Uri(baseUri, endOfUri);
            try
            {
                HtmlWeb web = new HtmlWeb();
                //web.OverrideEncoding = Encoding.GetEncoding("ISO-8859-1");
                HtmlDocument doc = web.Load(Url);
                var hrefs = doc.DocumentNode.Descendants("article")
                                            .Select(article => article.Descendants("a").FirstOrDefault())
                                            .Where(a => a != null)
                                            .Select(a => a.GetAttributeValue("href", ""))
                                            .ToList();
                resultArticles = hrefs.Select(link => new Article(new Uri(baseUri, link), date, "origo")).ToList();

            }
            catch (Exception ex)
            {
                Task.FromException(ex);
            }
            return Task.FromResult(resultArticles.AsEnumerable());
        }
    }
}