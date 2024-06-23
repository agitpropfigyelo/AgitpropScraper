using System.Xml;
using Agitprop.Infrastructure;
using Agitprop.Infrastructure.Enums;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Scrapers.Index;
using HtmlAgilityPack;

namespace Agitprop.Scrapers;

public class DateBasedArchive
{
    protected static string GetDateBasedUrl(string urlBase, string current)
    {
        var currentUrl = new Uri(current);
        var nextDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var toParse = currentUrl.Segments[1..4].ToList();

        if (DateOnly.TryParse(string.Join(".", toParse.Select(x => x.Replace("/", ""))), out DateOnly date))
        {
            nextDate = date.AddDays(-1);
        }
        return $"{urlBase}/{nextDate.Year:D4}/{nextDate.Month:D2}/{nextDate.Day:D2}";
    }
}


public class SitemapLinkParser
{
    public List<string> GetLinks(string docString)
    {
        XmlDocument document = new XmlDocument();
        document.LoadXml(docString);

        XmlNodeList urlNodes = document.GetElementsByTagName("url");

        List<string> resultList = [];
        foreach (XmlElement urlNode in urlNodes)
        {
            resultList.Add(urlNode.ChildNodes[0]!.InnerText);
        }
        return resultList;
    }
}

public class SitemapArchivePaginator
{
    public string GetUrl(string currentUrl, HtmlDocument document)
    {
        var uri = new Uri(currentUrl);
        var currentDate = DateOnly.ParseExact(uri.Segments[^1].Replace("_sitemap.xml", ""), "yyyyMM");
        var nextJobDate = currentDate.AddMonths(-1);
        return $"{uri.GetLeftPart(UriPartial.Authority)}/{nextJobDate:yyyyMM}_sitemap.xml";
    }
}
