using Agitprop.ConsoleApp;
using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Infrastructure;

internal class Program
{
    private const int PARALELLISM_DEGREE = 4;
    private static int cancelPressCount = 0;
    private static readonly object lockObj = new();
    private static readonly CancellationTokenSource cts = new CancellationTokenSource();


    private static async Task Main(string[] args)
    {












        //old code
        var startJobFactory = new StartJobFactory();
        var logger = new FileLogger(@"C:\Users\Forty\Repos\agitprop2\logs");
        var proxyProvider = new ProxyScrapeProxyProvider();
        var dbService = new SurrealDBProvider(logger);
        var configStore = new InMemoryScraperConfigStore();
        var nerService = new NamedEntityRecognizer("http://127.0.0.1:5000/", logger);
        var scb = new ScraperConfigBuilder();
        var spiderBuilder = new SpiderBuilder();
        var sink = new AgitpropSink(nerService, dbService, logger);
        var cookieStorage = new InMemoryCookieStorage();
        var scheduler = new InMemoryScheduler();
        int ScrapingLimit = 10 + (int)await dbService.GetVisitedLinksCount();

        Console.WriteLine("Hello, World!");
        Console.WriteLine($"Limit: {ScrapingLimit}");
        Console.WriteLine($"Parallelism: {PARALELLISM_DEGREE}");
        Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

        //using var logger = new ColorConsoleLogger();

        scb.SetHeadless(false);
        scb.SetPageCrawlLimit(ScrapingLimit);
        //scb.AddStartJobs(Enum.GetValues(typeof(NewsSites)).Cast<NewsSites>().Select(s => startJobFactory.GetAgitpropScrapingJob(s)).ToList());
        scb.AddStartJob(startJobFactory.GetAgitpropScrapingJob(NewsSites.Alfahir));

        await proxyProvider.Initialization;
        await dbService.Initialize();
        await dbService.GetVisitedLinksCount();


        await configStore.CreateConfigAsync(scb.Build());
        spiderBuilder.WithConfigStorage(configStore);
        spiderBuilder.WithLinkTracker(dbService);
        spiderBuilder.WithLogger(logger);
        spiderBuilder.WithSink(sink);
        spiderBuilder.WithCookieStorage(cookieStorage);
        spiderBuilder.WithProxies(proxyProvider);

        var engine = new ScraperEngine()
        {
            ConfigStorage = configStore,
            Scheduler = scheduler,
            Spider = spiderBuilder.Build(),
            Logger = logger,
            ParallelismDegree = PARALELLISM_DEGREE,

        };
        try
        {
            await engine.RunAsync(cts.Token);

        }
        catch (Exception ex)
        {
            Console.WriteLine("----EXCEPTION----");
            Console.WriteLine(ex.Message);
            Console.WriteLine("StackTrace");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine("----DONE----");

    }
    private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        lock (lockObj)
        {
            cancelPressCount++;

            if (cancelPressCount == 1)
            {
                // Prevent the process from terminating immediately
                e.Cancel = true;

                Console.WriteLine("Ctrl+C pressed. Stopping tasks...");

                // Signal cancellation to the tasks
                cts.Cancel();
            }
            else if (cancelPressCount == 2)
            {
                Console.WriteLine("Ctrl+C pressed again. Forcing shutdown.");

                // Allow the process to terminate immediately
                e.Cancel = false;
            }
        }
    }
}