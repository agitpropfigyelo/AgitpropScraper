using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Sinks.Newsfeed.Scrapers;

namespace Agitprop.Sinks.Newsfeed.Factories;

/// <summary>
/// Provides factory methods for creating scraping jobs for articles and archives.
/// </summary>
public static class ScrapingJobFactory
{
    /// <summary>
    /// Creates a scraping job for a specific article.
    /// </summary>
    /// <param name="source">The source of the article.</param>
    /// <param name="url">The URL of the article.</param>
    /// <returns>A <see cref="ScrapingJob"/> configured for the article.</returns>
    public static ScrapingJob GetArticleScrapingJob(NewsSites source, string url)
    {
        return new ScrapingJob
        {
            Url = url,
            PageCategory = PageCategory.TargetPage,
            PageType = PageType.Static,
            LinkParsers = [ArchiveLinkParserFactory.GetLinkParser(source)],
            ContentParsers = [ContentParserFactory.GetContentParser(source)]
        };
    }

    /// <summary>
    /// Creates a scraping job for an archive page.
    /// </summary>
    /// <param name="source">The source of the archive.</param>
    /// <param name="url">The URL of the archive page.</param>
    /// <returns>A <see cref="ScrapingJob"/> configured for the archive page.</returns>
    public static ScrapingJob GetArchiveScrapingJob(NewsSites source, string url)
    {
        return new ScrapingJob
        {
            Url = url,
            PageCategory = PageCategory.PageWithPagination,
            PageType = source == NewsSites.NegyNegyNegy ? PageType.Dynamic : PageType.Static,
            Actions = source == NewsSites.NegyNegyNegy ? [new(PageActionType.Execute, new NegynegynegyArchiveScrollAction())] : default,
            LinkParsers = [ArchiveLinkParserFactory.GetLinkParser(source)],
            ContentParsers = [ContentParserFactory.GetContentParser(source)],
            Pagination = PaginatorFactory.GetPaginator(source)
        };
    }
}
