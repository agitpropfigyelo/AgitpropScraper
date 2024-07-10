namespace Agitprop.Core;

public record ScraperConfig(
    IEnumerable<ScrapingJob> StartJobs,
    IEnumerable<string> UrlBlackList,
    IEnumerable<string> DomainWhiteList,
    DateOnly? SearchDate,
    int PageCrawlLimit,
    bool Headless
);
