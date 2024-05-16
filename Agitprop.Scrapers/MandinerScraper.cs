using System.Globalization;
using System.Net;
using System.Xml;
using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class MandinerScraper : INewsSiteScraper
{
private readonly Uri baseUri = new Uri("https://www.mandiner.hu");

    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='article-page-title']");
        string titleText = titleNode.InnerText.Trim()+" ";

        // Select nodes with class "article-lead"
        var leadNode = document.DocumentNode.SelectSingleNode("//p[@class='article-page-lead']");
        string leadText = leadNode.InnerText.Trim()+" ";

        // Select nodes with tag "origo-wysiwyg-box"
        var boxNodes = document.DocumentNode.SelectNodes("//man-wysiwyg-box");
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
