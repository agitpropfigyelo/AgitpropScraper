using Agitprop.Infrastructure.Enums;
using Agitprop.Infrastructure.Interfaces;

namespace Agitprop.Infrastructure;

public class ScrapingJobBuilder()
{
    private string Url = string.Empty;
    private PageCategory PageCategory = PageCategory.TransitPage;
    private PageType PageType = PageType.Static;
    private List<PageAction> Actions = [];
    private List<IContentParser> ContentParsers = [];
    private List<ILinkParser> LinkParsers = [];
    public IPaginator? Pagination = null;

    public ScrapingJob Build()
    {
        if (PageCategory == PageCategory.PageWithPagination && Pagination == null) throw new InvalidOperationException("A page with pagination must have a paginator!");
        return new ScrapingJob
        {
            Url = this.Url,
            PageCategory = this.PageCategory,
            PageType = this.PageType,
            Actions = this.Actions,
            ContentParsers = this.ContentParsers,
            LinkParsers = this.LinkParsers,
            Pagination = this.Pagination,
        };
    }
    public ScrapingJobBuilder SetUrl(string url)
    {
        Url = url;
        return this;
    }
    public ScrapingJobBuilder SetPageCategory(PageCategory pageCategory)
    {
        PageCategory = pageCategory;
        return this;
    }

    public ScrapingJobBuilder SetPageType(PageType pageType)
    {
        PageType = pageType;
        return this;
    }
    public ScrapingJobBuilder AddPageAction(PageAction action)
    {
        Actions.Add(action);
        return this;
    }

    public ScrapingJobBuilder AddContentParser(IContentParser contentParser)
    {
        ContentParsers.Add(contentParser);
        return this;
    }
    public ScrapingJobBuilder AddLinkParser(ILinkParser linkParser)
    {
        LinkParsers.Add(linkParser);
        return this;
    }

    public ScrapingJobBuilder AddPagination(IPaginator paginator)
    {
        this.Pagination = paginator;
        return this;
    }

    public ScrapingJobBuilder AddJob(ScrapingJob job)
    {
        if (job.Url == this.Url) throw new InvalidOperationException("");
        if (job.PageType == this.PageType) throw new InvalidOperationException("");
        if (job.PageCategory == this.PageCategory) throw new InvalidOperationException("");
        this.LinkParsers.AddRange(job.LinkParsers);
        this.ContentParsers.AddRange(job.ContentParsers);
        return this;
    }

}