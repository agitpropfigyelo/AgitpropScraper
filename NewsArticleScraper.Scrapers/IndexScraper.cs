using System.Globalization;
using System.Xml;
using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class IndexScraper : INewsSiteScraper
{
private readonly Uri baseUri = new Uri("https://www.index.hu");

    public string GetArticleContent(HtmlDocument document)
    {
        var titleNode = document.DocumentNode.SelectSingleNode("//div[@class='content-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var leadNode = document.DocumentNode.SelectSingleNode("//div[@class='lead']");
        string leadText = leadNode.InnerText.Trim() + " ";

        var boxNode = document.DocumentNode.SelectSingleNode("//div[@class='cikk-torzs']");

        var toRemove = boxNode.SelectNodes("//div[contains(@class, 'cikk-bottom-text-ad')]");
        foreach (var item in toRemove)
        {
            item.Remove();
        }

        string boxText = boxNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + leadText + boxText;

        return Helper.CleanUpText(concatenatedText);
    }

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        var suffix = $"sitemap/cikkek_{dateIn:yyyyMM}.xml";
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
                    DateTime timestamp = DateTime.Parse(childNodes[1]!.InnerText,CultureInfo.InvariantCulture);
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
