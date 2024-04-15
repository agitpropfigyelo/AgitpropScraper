using System.Net;
using System.Text;
using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class KuruczinfoScraper : INewsSiteScraper
{
    private readonly Uri baseUrl = new("https://kuruc.info/");
    public KuruczinfoScraper()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    public string GetArticleContent(HtmlDocument document)
    {
        //TODO: a weboldal iso-8859-2 encoding-al van, valahogy ki kéne kupálni, hogy jó legyen
        //Convert this mofo to utf8, like all other normal newssite is, also fucked up using of html encoding
        //document= document.LoadHtml();

        // Select nodes with class "article-title"       
        var titleNode = document.DocumentNode.SelectSingleNode("//div[@class='focikkheader']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var articleNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'cikktext')]");
        string articleText = Helper.ConcatenateNodeText(articleNodes);


        // Concatenate all text
        string concatenatedText = WebUtility.HtmlDecode(titleText + articleText);

        return Helper.CleanUpText(concatenatedText);
    }

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        List<string> resultArticles = [];
        int pageNum = 1;
        bool moveToNextPage = true;

        try
        {
            while (moveToNextPage)
            {
                string archivePath = $"/to/1/{pageNum}/";
                pageNum += 20;
                Uri url = new(baseUrl, archivePath);
                using (HttpClient client = new HttpClient())
                {
                    string htmlContent = await client.GetStringAsync(url);

                    HtmlDocument doc = new();
                    doc.LoadHtml(htmlContent);

                    List<ArchiveArticleInfo> articleInfos = [];
                    HtmlNodeCollection articleNodes = doc.DocumentNode.SelectNodes(".//div[@class='alcikkheader']/a");
                    HtmlNodeCollection dateNodes = doc.DocumentNode.SelectNodes(".//div[@class='cikkdatum']");
                    dateNodes.RemoveAt(0);
                    var first = DateTimeOffset.Parse(dateNodes.First().InnerText.Split("::")[1].Trim());
                    var last = DateTimeOffset.Parse(dateNodes.Last().InnerText.Split("::")[1].Trim());
                    if (last.Date > dateIn.Date) continue;
                    if (first.Date < dateIn.Date) break;
                    foreach ((HtmlNode articleNode, DateTimeOffset date) in articleNodes.Zip(dateNodes.Select(item => DateTimeOffset.Parse(item.InnerText.Split("::")[1].Trim()))))
                    {
                        if(date.Date==dateIn.Date){
                            resultArticles.Add(articleNode.GetAttributeValue("href",""));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Rethrow the exception as a task result
            throw new InvalidOperationException("Error occurred while fetching articles", ex);
            //add logging
        }


        return resultArticles;
    }
}
