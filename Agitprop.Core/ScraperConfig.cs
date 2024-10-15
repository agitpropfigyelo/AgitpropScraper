using Agitprop.Infrastructure.Interfaces;

namespace Agitprop.Core;

public record ScraperConfig(
    IEnumerable<ScrapingJob> StartJobs,
    IEnumerable<string> DomainBlackList,
    IEnumerable<string> DomainWhiteList,
    DateOnly? SearchDate,
    int PageCrawlLimit,
    int Parallelism,
    bool Headless
);
