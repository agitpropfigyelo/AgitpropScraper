namespace Agitprop.Sinks.Newsfeed.Scrapers.ArchivePaginators;

internal class MetropolArchivePaginator : SitemapArchivePaginator, IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new NewsfeedJobDescrpition
        {
            Url = new Uri(GetUrl(currentUrl, document)).ToString(),
            Type = PageContentType.Archive,
        };
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return Task.FromResult(GetNextPage(currentUrl, doc));
    }
}
