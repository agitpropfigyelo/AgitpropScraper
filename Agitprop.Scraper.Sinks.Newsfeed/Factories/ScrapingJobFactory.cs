using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Factories;

namespace Agitprop.Scraper.Sinks.Newsfeed.Factories;

internal static class ScrapingJobFactory
{

    public static ScrapingJob GetArticleScrapingJob(NewsSites source, string url)
    {
        return new ScrapingJob
        {
            Url = url,
            PageCategory = PageCategory.TargetPage,
            PageType = PageType.Static,
            LinkParsers = [LinkParserFactory.GetLinkParser(source)],
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
            Actions = source == NewsSites.NegyNegyNegy ? [new(PageActionType.Execute, new Scrapers.Negynegynegy.NegynegynegyArchiveScrollAction())] : default,
            LinkParsers = [LinkParserFactory.GetLinkParser(source)],
            ContentParsers = [ContentParserFactory.GetContentParser(source)],
            Pagination = PaginatorFactory.GetPaginator(source)
        };
    }
}
