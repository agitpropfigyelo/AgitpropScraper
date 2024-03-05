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


        public Task<IEnumerable<Article>> GetArticlesForDayAsync(DateTime dateIn)
        {
            suffix = $"{dateIn:yyyyMM}_sitemap.xml";
            Uri weblink = new(baseUri!, suffix);
            string sitemapUrl = weblink.ToString();
            List<Article> resultArticles = [];

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = client.GetAsync(sitemapUrl).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException($"Cannot access site: {sitemapUrl}");
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        string xmlContent = response.Content.ReadAsStringAsync().Result;

                        // Load XML content into HtmlAgilityPack's HtmlDocument
                        HtmlDocument htmlDocument = new HtmlDocument();
                        htmlDocument.LoadHtml(xmlContent);

                        XmlDocument documen = new();
                        documen.LoadXml(xmlContent);
                        XmlNodeList akarmi = documen.GetElementsByTagName("url");
                        foreach (XmlElement item in akarmi)
                        {
                            XmlNodeList asd = item.ChildNodes;
                            string location = asd[0]!.InnerText;
                            DateTime timestamp = DateTime.Parse(asd[1]!.InnerText);
                            if (timestamp.Date == dateIn.Date)
                            {
                                Article tmp = new(location, dateIn, source);
                                resultArticles.Add(tmp);
                            }
                        }
                        // Process the sitemap XML document using HtmlAgilityPack
                    }
                }
                catch (Exception ex)
                {
                    Task.FromException(ex);
                }
                return Task.FromResult(resultArticles as IEnumerable<Article>);
            }
        }
    }

}