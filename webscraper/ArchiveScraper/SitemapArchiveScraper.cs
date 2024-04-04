using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using HtmlAgilityPack;
using Microsoft.VisualBasic;

namespace webscraper
{
    public class SitemapArchiveScraper : IArchiveScraperService
    {
        private Uri baseUri { get; set; }
        private string? suffix;
        private readonly string source;

        public SitemapArchiveScraper(string uriIn, string sourceIn)
        {
            this.source = sourceIn;
            this.baseUri = new(uriIn);
        }


        public SitemapArchiveScraper(Uri uriIn, string sourceIn)
        {
            this.source = sourceIn;
            this.baseUri = uriIn;
        }


        public async Task<IEnumerable<Article>> GetArticlesForDayAsync(DateTime dateIn)
        {
            suffix = $"{dateIn:yyyyMM}_sitemap.xml";
            Uri weblink = new(baseUri!, suffix);
            string sitemapUrl = weblink.ToString();

            List<Article> resultArticles = new List<Article>(); // Initialize the list

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(sitemapUrl).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException($"Cannot access site: {sitemapUrl}");
                    }

                    string xmlContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    XmlDocument documen = new XmlDocument();
                    documen.LoadXml(xmlContent);

                    XmlNodeList urlNodes = documen.GetElementsByTagName("url");

                    foreach (XmlElement urlNode in urlNodes)
                    {
                        XmlNodeList childNodes = urlNode.ChildNodes;
                        string location = childNodes[0]!.InnerText;
                        DateTime timestamp = DateTime.Parse(childNodes[1]!.InnerText);
                        if (timestamp.Date == dateIn.Date)
                        {
                            Article tmp = new Article(location, dateIn, source);
                            resultArticles.Add(tmp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Rethrow the exception as a task result
                    throw new InvalidOperationException("Error occurred while fetching articles", ex);
                }
            }

            return resultArticles; // Return the list as a task result
        }

    }

}