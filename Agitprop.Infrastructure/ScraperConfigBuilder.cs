using Agitprop.Core;

namespace Agitprop.Infrastructure;

public class ScraperConfigBuilder()
{
    List<ScrapingJob> StartJobs = [];
    List<string> UrlBlackList = [];
    List<string> DomainWhiteList = [];
    int PageCrawlLimit = int.MaxValue;
    bool Headless = true;
    DateOnly Date = DateOnly.FromDateTime(DateTime.Now).AddDays(-1);
    public ScraperConfigBuilder AddStartJob(ScrapingJob job)
    {
        StartJobs.Add(job);
        return this;
    }
    public ScraperConfigBuilder AddStartJobs(List<ScrapingJob> jobs)
    {
        StartJobs.AddRange(jobs);
        return this;
    }

    public ScraperConfigBuilder AddBlacListedUrl(string url)
    {
        UrlBlackList.Add(url);
        return this;
    }
    public ScraperConfigBuilder AddWhiteListedDomain(string url)
    {
        DomainWhiteList.Add(url);
        return this;
    }
    public ScraperConfigBuilder SetPageCrawlLimit(int limit)
    {
        PageCrawlLimit = limit;
        return this;
    }
    public ScraperConfigBuilder SetHeadless(bool isHeadless)
    {
        Headless = isHeadless;
        return this;
    }

    public ScraperConfigBuilder SetDate(DateOnly date)
    {
        Date = date;
        return this;
    }

    public ScraperConfig Build()
    {
        return new ScraperConfig(this.StartJobs, this.UrlBlackList, this.DomainWhiteList, this.Date, this.PageCrawlLimit, this.Headless);
    }
}
