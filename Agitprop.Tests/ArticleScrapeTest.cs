using System.Text;
using Agitprop.Infrastructure;
using Agitprop.Core.Enums;
using Agitprop.Core.Factories;
using Agitprop.Core.Interfaces;
using Agitprop.Core;
using Agitprop.Scrapers.Factories;
using Microsoft.Extensions.Logging;
using Agitprop.Scrapers.Tests;

namespace NewsArticleScraper.Tests;

[Parallelizable(scope: ParallelScope.All)]
public class ArticleScrapeTest
{

    private IContentParserFactory ContentParserFactory = new ContentParserFactory();
    private IPaginatorFactory PaginatorFactory = new PaginatorFactory();
    private ILinkParserFactory LinkParserFactory = new LinkParserFactory();
    private HttpClient webClient = new();
    public ScrapingJobFactory ScrapingJobFactory = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    //TODO: use local html files
    [TestCase(NewsSites.Origo, Links.ORIGO_ARTICLE, @".\Expected\origo.txt")]
    [TestCase(NewsSites.Ripost, Links.RIPOST_ARTICLE, @".\Expected\ripost.txt")]
    [TestCase(NewsSites.Mandiner, Links.MANDINER_ARTICLE, @".\Expected\mandiner.txt")]
    [TestCase(NewsSites.Metropol, Links.METROPOL_ARTICLE, @".\Expected\metropol.txt")]
    [TestCase(NewsSites.MagyarNemzet, Links.MAGYARNEMZET_ARTICLE, @".\Expected\magyarnemzet.txt")]
    [TestCase(NewsSites.PestiSracok, Links.PESTISRACOK_ARTICLE, @".\Expected\pestisracok.txt")]
    [TestCase(NewsSites.MagyarJelen, Links.MAGYARJELEN_ARTICLE, @".\Expected\magyarjelen.txt")]
    [TestCase(NewsSites.Kuruczinfo, Links.KURUCINFO_ARTICLE, @".\Expected\kuruczinfo.txt")]//TODO: fix encoding problem when using local html for kuruczinfo
    [TestCase(NewsSites.Alfahir, Links.ALFAHIR_ARTICLE, @".\Expected\alfahir.txt")]
    [TestCase(NewsSites.Huszonnegy, Links.HUSZONNEGY_ARTICLE, @".\Expected\huszonnegy.txt")]//TODO: Add test where there is lead to the article
    [TestCase(NewsSites.NegyNegyNegy, Links.NEGYNEGYNEGY_ARTICLE, @".\Expected\negynegynegy.txt")]
    [TestCase(NewsSites.HVG, Links.HVG_ARTICLE, @".\Expected\hvg.txt")]
    [TestCase(NewsSites.Telex, Links.TELEX_ARTICLE, @".\Expected\telex.txt")]
    [TestCase(NewsSites.RTL, Links.RTL_ARTICLE, @".\Expected\rtl.txt")]
    [TestCase(NewsSites.Index, Links.INDEX_ARTICLE, @".\Expected\index.txt")]
    [TestCase(NewsSites.Merce, Links.MERCE_ARTICLE, @".\Expected\merce.txt")]
    public async Task ContentParserTest(NewsSites source, string siteUrl, string expectedPath)
    {
        IContentParser contentParser = ContentParserFactory.GetContentParser(source);
        Assert.That(File.Exists(expectedPath), "Result txt missing");
        var document = await webClient.GetStringAsync(siteUrl);
        await webClient.GetStringAsync(siteUrl);
        var result = await contentParser.ParseContentAsync(document);
        Assert.That(result.Text, Is.EqualTo(File.ReadAllText(expectedPath)));
    }
    [TestCase(NewsSites.Origo, Links.ORIGO_ARCHIVE, "https://www.origo.hu/hirarchivum/2024/03/10")]
    [TestCase(NewsSites.Ripost, Links.RIPOST_ARCHIVE, "https://ripost.hu/202403_sitemap.xml")]
    [TestCase(NewsSites.Mandiner, Links.MANDINER_ARCHIVE, "https://mandiner.hu/202403_sitemap.xml")]
    [TestCase(NewsSites.Metropol, Links.METROPOL_ARCHIVE, "https://metropol.hu/202403_sitemap.xml")]
    [TestCase(NewsSites.MagyarNemzet, Links.MAGYARNEMZET_ARCHIVE, "https://magyarnemzet.hu/202403_sitemap.xml")]
    [TestCase(NewsSites.PestiSracok, Links.PESTISRACOK_ARCHIVE, "https://www.pestisracok.hu/2024/03/10")]
    [TestCase(NewsSites.MagyarJelen, Links.MAGYARJELEN_ARCHIVE_DATE, "https://magyarjelen.hu/2024/03/11/page/2/")]
    [TestCase(NewsSites.MagyarJelen, Links.MAGYARJELEN_ARCHIVE_PAGE, "https://magyarjelen.hu/2024/03/10")]
    [TestCase(NewsSites.Kuruczinfo, Links.KURUCZINFO_ARCHIVE, "https://kuruc.info/to/1/40/")]//TODO: fix encoding problem when using local html for kuruczinfo
    [TestCase(NewsSites.Alfahir, Links.ALFAHIR_ARCHIVE, "https://alfahir.hu/hirek/oldalak/2")]
    [TestCase(NewsSites.Huszonnegy, Links.HUSZONNEGY_ARCHIVE, "https://24.hu/2024/03/10")]//TODO: Add test where there is lead to the article
    [TestCase(NewsSites.NegyNegyNegy, Links.NEGYNEGYNEGY_ARCHIVE, "https://444.hu/2024/03/10")]
    [TestCase(NewsSites.HVG, Links.HVG_ARCHIVE, "http://hvg.hu/frisshirek/2024.03.10")]
    [TestCase(NewsSites.Telex, Links.TELEX_ARCHIVE, "https://telex.hu/legfrissebb?oldal=2")]
    [TestCase(NewsSites.RTL, Links.RTL_ARCHIVE, "https://rtl.hu/legfrissebb?oldal=2")]
    [TestCase(NewsSites.Index, Links.INDEX_ARCHIVE, "https://index.hu/sitemap/cikkek_200301.xml")]
    [TestCase(NewsSites.Merce, Links.MERCE_ARCHIVE, "https://merce.hu/2024/03/10")]
    public async Task ArchivePaginatorTest(NewsSites source, string siteUrl, string expectUrl)
    {
        IPaginator paginator = PaginatorFactory.GetPaginator(source);
        var document = await webClient.GetStringAsync(siteUrl);
        var result = await paginator.GetNextPageAsync(siteUrl, document);
        Assert.That(result.Url, Is.EqualTo(expectUrl));
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
        ScraperConfig conf = new ScraperConfigBuilder().SetHeadless(false).Build();
        var configStore = new InMemoryScraperConfigStore();
        await configStore.CreateConfigAsync(conf);
        SpiderBuilder sb = new();
        sb.WithCookieStorage(new InMemoryCookieStorage());
        sb.WithLinkTracker(new InMemoryVisitedLinkTracker());
        sb.WithLogger(new ColorConsoleLogger());
        sb.WithConfigStorage(configStore);
        var spider = sb.Build();
        var job = ScrapingJobFactory.GetArchiveScrapingJob(source, siteUrl);
        List<ScrapingJob> result = await spider.CrawlAsync(job);
        Assert.That(result.Count, Is.EqualTo(expectedCount));

    }

    [Test]
    public async Task NamedEntityRecognizer_IsAliveTest()
    {
        var ner = new NamedEntityRecognizer("http://127.0.0.1:5000/", null);
        Assert.That(await ner.PingAsync(), Is.EqualTo("OK"));
    }

    [Test]
    public async Task NamedEntityRecognizer_SendSingle()
    {
        var ner = new NamedEntityRecognizer("http://127.0.0.1:5000/", null);
        Assert.That(await ner.PingAsync(), Is.EqualTo("OK"), "NER service is offline");
        var idk = await ner.AnalyzeSingleAsync("Hajmési Péter elintonál Győrben ami Dávid és nem tudom mi ez a Szöveg, de azért írom, meg legyen benne olyan is hogy Telex meg Hős utca asd.");
        Assert.That(idk, Is.Not.Null);
    }
    [Test]
    public async Task AddMention()
    {
        var database = new SurrealDBProvider(null);
        await database.CreateMentionsAsync("hahaha.hu", new ContentParserResult()
        {
            PublishDate = DateTime.Now,
            SourceSite = NewsSites.Ripost,
            Text = "Ez egy szöveg"
        }, new NamedEntityCollection()
        {
            LOC = ["Csór", "Bábolna"],
            PER = ["Valaki Máska"]
        });
        Assert.Pass();
    }
}