using System.Xml;
using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class RipostScraper : INewsSiteScraper
{
    private readonly Uri baseUri = new Uri("https://www.ripost.hu");

    public string GetArticleContent(HtmlDocument document)
    {
        string text = document.DocumentNode.SelectSingleNode("/html/body/app-root/app-base/div[2]/app-article-page/section").InnerText;
        if (string.IsNullOrEmpty(text)) throw new InvalidOperationException("Not able to scrape site");
        return text.Replace("&nbsp;", "");
    }
    
    public async Task<List<string>> GetArticlesAsync(DateTime dateIn)
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

                XmlDocument documen = new XmlDocument();
                documen.LoadXml(response);

                XmlNodeList urlNodes = documen.GetElementsByTagName("url");

                foreach (XmlElement urlNode in urlNodes)
                {
                    XmlNodeList childNodes = urlNode.ChildNodes;
                    string location = childNodes[0]!.InnerText;
                    DateTime timestamp = DateTime.Parse(childNodes[1]!.InnerText);
                    if (timestamp.Date == dateIn.Date)
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
