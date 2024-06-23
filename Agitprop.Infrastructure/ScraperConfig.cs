using Agitprop.Infrastructure.Enums;

namespace Agitprop.Infrastructure;

public record ScraperConfig(
    IEnumerable<ScrapingJob> StartJobs,
    IEnumerable<string> UrlBlackList,
    IEnumerable<string> DomainWhiteList,
    DateOnly? SearchDate,
    int PageCrawlLimit,
    bool Headless
);
