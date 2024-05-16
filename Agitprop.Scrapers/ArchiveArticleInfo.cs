using HtmlAgilityPack;

namespace NewsArticleScraper.Scrapers;

public class ArchiveArticleInfo(string urlToArticle, DateTimeOffset publishDate)
{
    public string UrlToArticle { get; init; } = urlToArticle;
    public DateTimeOffset PublishDate { get; init; } = publishDate;
}