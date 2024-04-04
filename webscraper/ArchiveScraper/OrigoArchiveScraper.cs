using HtmlAgilityPack;

namespace webscraper
{
    public class OrigoArchiveScraper : IArchiveScraperService
    {
        private readonly Uri baseUri = new Uri("https://www.origo.hu");

        public async Task<IEnumerable<Article>> GetArticlesForDayAsync(DateTime date)
        {
            List<Article> resultArticles = new List<Article>();
            string endOfUri = $"/hir-archivum/{date.Year}/{date:yyyyMMdd}.html";
            Uri url = new Uri(baseUri, endOfUri);

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string htmlContent = await client.GetStringAsync(url).ConfigureAwait(false);

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    var hrefs = doc.DocumentNode.Descendants("article")
                        .Select(article => article.Descendants("a").FirstOrDefault())
                        .Where(a => a != null)
                        .Select(a => a.GetAttributeValue("href", ""))
                        .ToList();
                    resultArticles = hrefs.Select(link => new Article(new Uri(baseUri, link), date, "origo")).ToList();
                }
            }
            catch (Exception ex)
            {
                // Rethrow the exception as a task result
                throw new InvalidOperationException("Error occurred while fetching articles", ex);
            }

            return resultArticles;
        }
    }
}
