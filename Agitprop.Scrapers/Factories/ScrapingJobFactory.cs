using Agitprop.Infrastructure;
using Agitprop.Core.Enums;
using Agitprop.Core.Factories;
using Agitprop.Core;

namespace Agitprop.Scrapers.Factories;

public class ScrapingJobFactory
{
    private IContentParserFactory ContentParserFactory = new ContentParserFactory();
    private IPaginatorFactory PaginatorFactory = new PaginatorFactory();
    private ILinkParserFactory LinkParserFactory = new LinkParserFactory();

    public ScrapingJob GetArticleScrapingJob(NewsSites source, string url)
    {
        var ScrapingJobBuilder = new ScrapingJobBuilder();
        ScrapingJobBuilder.SetUrl(url);
        ScrapingJobBuilder.SetPageCategory(PageCategory.TargetPage);
        ScrapingJobBuilder.AddLinkParser(LinkParserFactory.GetLinkParser(source));
        ScrapingJobBuilder.AddContentParser(ContentParserFactory.GetContentParser(source));

        return ScrapingJobBuilder.Build();
    }
    public ScrapingJob GetArchiveScrapingJob(NewsSites source, string url)
    {
        var ScrapingJobBuilder = new ScrapingJobBuilder();
        ScrapingJobBuilder.SetUrl(url);
        ScrapingJobBuilder.SetPageCategory(PageCategory.PageWithPagination);
        ScrapingJobBuilder.AddLinkParser(LinkParserFactory.GetLinkParser(source));
        ScrapingJobBuilder.AddPagination(PaginatorFactory.GetPaginator(source));
        ScrapingJobBuilder.AddContentParser(ContentParserFactory.GetContentParser(source));

        if (source == NewsSites.NegyNegyNegy)
        {
            ScrapingJobBuilder.SetPageType(PageType.Dynamic);
            PageAction action = new(PageActionType.Execute, new Negynegynegy.ArchiveScrollAction());
            ScrapingJobBuilder.AddPageAction(action);
        }
        else
        {
            ScrapingJobBuilder.SetPageType(PageType.Static);

        }

        return ScrapingJobBuilder.Build();
    }
}
