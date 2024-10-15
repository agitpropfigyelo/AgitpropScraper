using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure;
using Agitprop.Infrastructure.InMemory;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Infrastructure.PageLoader;
using Agitprop.Infrastructure.PageRequester;
using Agitprop.Infrastructure.SurrealDB;
using Agitprop.Worker;
using Polly;
using Polly.Retry;
using PuppeteerSharp;

internal class Program
{
    private static void Main(string[] args)
    {
        var startJobFactory = new StartJobFactory();

        var ScraperConfig = new ScraperConfig(
            StartJobs: [startJobFactory.GetAgitpropScrapingJob(NewsSites.NegyNegyNegy)],
            // StartJobs: Enum.GetValues(typeof(NewsSites)).Cast<NewsSites>().Select(s => startJobFactory.GetAgitpropScrapingJob(s)),
            DomainBlackList: [],
            DomainWhiteList: [],
            SearchDate: DateOnly.FromDateTime(DateTime.Now).AddDays(-1),
            PageCrawlLimit: 1600,
            Parallelism: 4,
            Headless: true);

        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration.AddJsonFile("appsettings.json", false);

        builder.Services.AddResiliencePipeline("Spider", static builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<NavigationException>().Handle<HttpRequestException>(),
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(5),
                MaxRetryAttempts = 4,
                UseJitter = true,
            });
        });

        builder.Services.AddSurreal(builder.Configuration.GetConnectionString("SurrealDB") ?? throw new MissingConfigurationValueException("Missing config for SurrealDB"));

        builder.Services.AddTransient<IAgitpropDataBaseService, AgitpropDBService>();
        builder.Services.AddTransient<INamedEntityRecognizer, NamedEntityRecognizer>();
        builder.Services.AddTransient<ISink, AgitpropSink>();

        builder.Services.AddHostedService<ScraperEngine>();
        builder.Services.AddSingleton(ScraperConfig);
        builder.Services.AddTransient<IScheduler, Scheduler>();
        builder.Services.AddLogging((builder) =>
        {
            builder.AddConsole();
        });
        builder.Services.AddTransient<ISpider, Spider>();
        builder.Services.AddTransient<ILinkTracker, Agitprop.Infrastructure.SurrealDB.VisitedLinkTracker>();

        builder.Services.AddTransient<IBrowserPageLoader, Agitprop.Infrastructure.Puppeteer.PuppeteerPageLoader>();
        builder.Services.AddTransient<ICookiesStorage, CookieStorage>();

        builder.Services.AddTransient<IStaticPageLoader, HttpStaticPageLoader>();
        //builder.Services.AddTransient<IPageRequester, PageRequester>();
        builder.Services.AddTransient<IPageRequester, RotatingProxyPageRequester>();
        builder.Services.AddTransient<IProxyProvider, ProxyScrapeProxyProvider>();



        var host = builder.Build();
        host.Run();
    }
}