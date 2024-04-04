using System.Net;
using System.Text;
using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class KuruczinfoScraper : INewsSiteScraper
{
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
        string concatenatedText =WebUtility.HtmlDecode(titleText + articleText);

        return Helper.CleanUpText(concatenatedText);
    }

    public Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        throw new NotImplementedException();
    }
}
