using Agitprop.Core;
using Agitprop.Infrastructure;
using Agitprop.Core.Interfaces;
using Agitprop.Scrapers.Tests;
using Agitprop.Infrastructure.Enums;

namespace NewsArticleScraper.Tests;

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
        ScrapingJobBuilder.AddLinkParser(this.LinkParserFactory.GetLinkParser(source));
        ScrapingJobBuilder.AddContentParser(this.ContentParserFactory.GetContentParser(source));

        return ScrapingJobBuilder.Build();
    }
    public ScrapingJob GetArchiveScrapingJob(NewsSites source, string url)
    {
        var ScrapingJobBuilder = new ScrapingJobBuilder();
        ScrapingJobBuilder.SetUrl(url);
        ScrapingJobBuilder.SetPageCategory(PageCategory.PageWithPagination);
        ScrapingJobBuilder.AddLinkParser(this.LinkParserFactory.GetLinkParser(source));
        ScrapingJobBuilder.AddPagination(this.PaginatorFactory.GetPaginator(source));
        ScrapingJobBuilder.AddContentParser(this.ContentParserFactory.GetContentParser(source));

        if (source == NewsSites.NegyNegyNegy)
        {
            ScrapingJobBuilder.SetPageType(PageType.Dynamic);
            PageAction action=new(PageActionType.Execute,new Agitprop.Scrapers.Negynegynegy.ArchiveScrollAction());
            ScrapingJobBuilder.AddPageAction(action);
        }
        else
        {
            ScrapingJobBuilder.SetPageType(PageType.Static);

        }

        return ScrapingJobBuilder.Build();
    }
}
