using Agitprop.Core.Enums;
using Agitprop.Infrastructure;
using Agitprop.Scrapers.Factories;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await DoValid();

    }

    private static async Task DoValid()
    {
        Console.WriteLine("Hello, World!");
        using var logger = new ColorConsoleLogger();

        ScraperConfigBuilder scb = new();
        scb.SetHeadless(true);
        scb.SetPageCrawlLimit(5);
        scb.AddStartJob(new ScrapingJobFactory().GetArchiveScrapingJob(NewsSites.Alfahir, "https://alfahir.hu/hirek/oldalak/1"));


        //Add starter jobs
        //try alfahir
        var proxyProvider = new ProxyScrapeProxyProvider();
        await proxyProvider.Initialization;
        var dbService = new SurrealDBProvider(logger);
        await dbService.Initialize();
        var nerService = new NamedEntityRecognizer("http://127.0.0.1:5000/", logger);


        var configStore = new InMemoryScraperConfigStore();
        await configStore.CreateConfigAsync(scb.Build());
        DateTime date = new(2024, 03, 11);

        var spiderBuilder = new SpiderBuilder();
        spiderBuilder.WithConfigStorage(configStore);
        spiderBuilder.WithLinkTracker(dbService);
        spiderBuilder.WithLogger(logger);
        spiderBuilder.WithSink(new AgitpropSink(nerService, dbService, logger));
        spiderBuilder.WithCookieStorage(new InMemoryCookieStorage());
        spiderBuilder.WithProxies(proxyProvider);

        var engine = new ScraperEngine()
        {
            ConfigStorage = configStore,
            Scheduler = new InMemoryScheduler(),
            Spider = spiderBuilder.Build(),
            Logger = logger,
            ParallelismDegree = 1,

        };
        await engine.RunAsync();

        System.Console.WriteLine("----DONE----");

    }
}