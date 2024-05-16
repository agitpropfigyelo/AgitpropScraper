using System.Globalization;
using System.Xml;
using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class RipostScraper : INewsSiteScraper
{
    private readonly Uri baseUri = new Uri("https://www.ripost.hu");

    public string GetArticleContent(HtmlDocument document)
    {
        var titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var leadNode = document.DocumentNode.SelectSingleNode("//div[@class='article-page-lead']");
        string leadText = leadNode.InnerText.Trim() + " ";

        var boxNodes = document.DocumentNode.SelectNodes("//app-wysiwyg-box");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

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