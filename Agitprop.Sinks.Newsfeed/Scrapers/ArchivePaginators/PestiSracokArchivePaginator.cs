namespace Agitprop.Sinks.Newsfeed.Scrapers.ArchivePaginators;

internal class PestiSracokArchivePaginator : DateBasedArchive, IPaginator
{
    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        return Task.FromResult(new NewsfeedJobDescrpition
        {
            Url = new Uri(GetDateBasedUrl("https://www.pestisracok.hu", currentUrl)).ToString(),
            Type = PageContentType.Archive,
        } as ScrapingJobDescription);
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return GetNextPageAsync(currentUrl, doc);
    }
}
