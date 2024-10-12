using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure;
using Agitprop.Infrastructure.InMemory;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Infrastructure.PageLoader;
using Agitprop.Infrastructure.PageRequester;
using Agitprop.Worker;

internal class Program
{
    private static void Main(string[] args)
    {
        var startJobFactory = new StartJobFactory();

        var scb = new ScraperConfigBuilder();
        scb.SetHeadless(false);
        scb.AddStartJob(startJobFactory.GetAgitpropScrapingJob(NewsSites.Alfahir));
        //scb.AddStartJobs(Enum.GetValues(typeof(NewsSites)).Cast<NewsSites>().Select(s => startJobFactory.GetAgitpropScrapingJob(s)).ToList());



        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<ScraperEngine>();
        builder.Services.AddSingleton<ScraperConfig>(scb.Build());
        builder.Configuration.AddJsonFile("appsettings.json", false);
        builder.Services.AddTransient<IScheduler, Scheduler>();
        builder.Services.AddLogging((builder) =>
        {
            builder.AddConsole();
        });
        builder.Services.AddTransient<ISpider, Spider>();
        builder.Services.AddTransient<ILinkTracker, VisitedLinkTracker>();

        builder.Services.AddTransient<IBrowserPageLoader, Agitprop.Infrastructure.Puppeteer.PuppeteerPageLoader>();
        builder.Services.AddTransient<ICookiesStorage, CookieStorage>();

        builder.Services.AddTransient<IStaticPageLoader, HttpStaticPageLoader>();
        builder.Services.AddTransient<IPageRequester, RotatingProxyPageRequester>();
        builder.Services.AddTransient<IProxyProvider, ProxyScrapeProxyProvider>();



        var host = builder.Build();
        host.Run();
    }
}