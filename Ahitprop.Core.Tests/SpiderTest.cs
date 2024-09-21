using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Infrastructure;
using Agitprop.Scrapers.Factories;

namespace Ahitprop.Core.Tests;

[TestFixture(true, Category = "With Proxies")]
[TestFixture(false, Category = "No Proxies")]
public class SpiderTest
{
    private bool withProxy;
    private ScrapingJobFactory jobFactory;
    InMemoryScraperConfigStore configStore;
    private Spider spider;
    public SpiderTest(bool _withProxy)
    {
        withProxy = _withProxy;
    }
    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        jobFactory = new ScrapingJobFactory();
        ScraperConfig conf = new ScraperConfigBuilder().SetHeadless(false).Build();
        configStore = new InMemoryScraperConfigStore();
        await configStore.CreateConfigAsync(conf);
    }
    [SetUp]
    public void Setup()
    {
        SpiderBuilder sb = new();
        sb.WithCookieStorage(new InMemoryCookieStorage());
        sb.WithLinkTracker(new InMemoryVisitedLinkTracker());
        sb.WithLogger(new ColorConsoleLogger());
        sb.WithConfigStorage(configStore);
        if (withProxy) sb.WithProxies(new ProxyScrapeProxyProvider());
        spider = sb.Build();
    }

    [TestCase(NewsSites.Origo, Links.ORIGO_ARCHIVE, 101)]
    [TestCase(NewsSites.Ripost, Links.RIPOST_ARCHIVE, 2284)]
    [TestCase(NewsSites.Mandiner, Links.MANDINER_ARCHIVE, 3153)]
    [TestCase(NewsSites.Metropol, Links.METROPOL_ARCHIVE, 1960)]
    [TestCase(NewsSites.MagyarNemzet, Links.MAGYARNEMZET_ARCHIVE, 4165)]
    [TestCase(NewsSites.PestiSracok, Links.PESTISRACOK_ARCHIVE, 44)]
    [TestCase(NewsSites.MagyarJelen, Links.MAGYARJELEN_ARCHIVE_DATE, 11)]
    [TestCase(NewsSites.MagyarJelen, Links.MAGYARJELEN_ARCHIVE_PAGE, 3)]
    [TestCase(NewsSites.Kuruczinfo, Links.KURUCZINFO_ARCHIVE, 21)]//TODO: fix encoding problem when using local html for kuruczinfo
    [TestCase(NewsSites.Alfahir, Links.ALFAHIR_ARCHIVE, 10)]
    [TestCase(NewsSites.Huszonnegy, Links.HUSZONNEGY_ARCHIVE, 25)]//TODO: Add test where there is lead to the article
    [TestCase(NewsSites.NegyNegyNegy, Links.NEGYNEGYNEGY_ARCHIVE, 56)]
    [TestCase(NewsSites.HVG, Links.HVG_ARCHIVE, 41)]
    [TestCase(NewsSites.Telex, Links.TELEX_ARCHIVE, 12)]
    [TestCase(NewsSites.RTL, Links.RTL_ARCHIVE, 51)]
    [TestCase(NewsSites.Index, Links.INDEX_ARCHIVE, 2462)]
    [TestCase(NewsSites.Merce, Links.MERCE_ARCHIVE, 5)]
    public async Task TestWithSpider(NewsSites source, string siteUrl, int expectedCount)
    {
        var job = jobFactory.GetArchiveScrapingJob(source, siteUrl);
        List<ScrapingJob> result = await spider.CrawlAsync(job);
        Assert.That(result.Count, Is.EqualTo(expectedCount));

    }
}