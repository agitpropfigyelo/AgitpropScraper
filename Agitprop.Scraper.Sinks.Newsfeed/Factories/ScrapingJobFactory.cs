using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Scraper.Sinks.Newsfeed.Scrapers;

namespace Agitprop.Scraper.Sinks.Newsfeed.Factories;

public static class ScrapingJobFactory
{

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
