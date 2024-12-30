using Agitprop.Core.Enums;
using Agitprop.Core.Factories;
using Agitprop.Core;

namespace Agitporp.Scraper.Sinks.Newsfeed.Factories;

public class ScrapingJobFactory
{
    private IContentParserFactory ContentParserFactory = new ContentParserFactory();
    private IPaginatorFactory PaginatorFactory = new PaginatorFactory();
    private ILinkParserFactory LinkParserFactory = new LinkParserFactory();

    public ScrapingJob GetArticleScrapingJob(NewsSites source, string url)
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
    public ScrapingJob GetArchiveScrapingJob(NewsSites source, string url)
    {
        return new ScrapingJob
        {
            Url = url,
            PageCategory = PageCategory.PageWithPagination,
            PageType = source == NewsSites.NegyNegyNegy ? PageType.Dynamic : PageType.Static,
            Actions = source == NewsSites.NegyNegyNegy ? [new(PageActionType.Execute, new Negynegynegy.ArchiveScrollAction())] : default,
            LinkParsers = [LinkParserFactory.GetLinkParser(source)],
            ContentParsers = [ContentParserFactory.GetContentParser(source)],
            Pagination = PaginatorFactory.GetPaginator(source)
        };
    }
}
