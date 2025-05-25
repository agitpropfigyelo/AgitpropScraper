namespace Agitprop.Sinks.Newsfeed.Scrapers.ArchiveLinkParsers;
internal class PestiSracokArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var result = doc.DocumentNode.SelectNodes("//*[@id='home-widget-wrap']/div/ul/li/div[1]/a")
                                     .Select(x => x.GetAttributeValue("href", ""))
                                     .Select(link => new NewsfeedJobDescrpition
                                     {
                                         Url = new Uri(link).ToString(),
                                         Type = PageContentType.Article,
                                     }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return GetLinksAsync(baseUrl, doc);
    }
}
