namespace Agitprop.Sinks.Newsfeed.Scrapers.ArchiveLinkParsers;

internal class KurucinfoArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var jobs = doc.DocumentNode.SelectNodes(".//div[@class='alcikkheader']/a")
                                   .Select(x => x.GetAttributeValue<string>("href", ""))
                                   .Select(url => new NewsfeedJobDescrpition
                                   {
                                       Url = new Uri(url).ToString(),
                                       Type = PageContentType.Article,
                                   }).Cast<ScrapingJobDescription>().ToList();

        return Task.FromResult(jobs);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return GetLinksAsync(baseUrl, doc);
    }
}
