using System.Text;
using Agitprop.Infrastructure;
using Agitprop.Core.Enums;
using Agitprop.Core.Factories;
using Agitprop.Core.Interfaces;
using Agitprop.Core;
using Agitprop.Scrapers.Factories;
using Microsoft.Extensions.Logging;

namespace NewsArticleScraper.Tests;

[Parallelizable(scope: ParallelScope.All)]
public class ArticleScrapeTest
{
    private const string ORIGO_ARTICLE = "https://www.origo.hu/itthon/2024/03/magyar-peter-az-uj-marki-zay-csak-meg-agresszivabban-baloldali";
    private const string RIPOST_ARTICLE = "https://ripost.hu/insider/2024/03/orban-viktor-levelet-kuldott-vlagyimir-putyinnak";
    private const string MANDINER_ARTICLE = "https://mandiner.hu/sport/2024/03/a-halalos-betegseggel-kuzdo-sven-goran-eriksson-leult-a-liverpool-kispadjara-koszontese-elkepeszto-volt-video";
    private const string METROPOL_ARTICLE = "https://metropol.hu/aktualis/2024/03/kitalalt-magyar-peterrol-egy-csaladi-barat-pszichiaterre-volt-szukseg";
    private const string MAGYARNEMZET_ARTICLE = "https://magyarnemzet.hu/belfold/2024/03/megszolalt-magyar-peter-csaladi-baratja-varga-judit";
    private const string PESTISRACOK_ARTICLE = "https://pestisracok.hu/lepeselonyben-a-globalista-elnokjelolt-szlovakiaban-de-csak-ket-het-mulva-lesz-dontes/";
    private const string MAGYARJELEN_ARTICLE = "https://magyarjelen.hu/kielezett-kuzdelmet-hozott-a-felvideki-allamfovalasztas-elso-forduloja/";
    private const string KURUCINFO_ARTICLE = "https://kuruc.info/r/6/271859/";
    private const string ALFAHIR_ARTICLE = "https://alfahir.hu/hirek/ismet-csokkentette-az-alapkamatot-a-matolcsy-vezette-jegybank";
    private const string HUSZONNEGY_ARTICLE = "https://24.hu/belfold/2024/03/29/titkos-megfigyeles-varga-judit-tuzson-bence-pegazus/";
    private const string NEGYNEGYNEGY_ARTICLE = "https://444.hu/2024/03/30/jockey-es-samantha-varja-most-az-erkezoket-varga-judit-balatonhenyei-hazanak-kapujaban";
    private const string HVG_ARTICLE = "https://hvg.hu/cegauto/20240331_egy-200-kilos-szarvas-zuhant-egy-autora-kesztolcnel-az-egyik-utas-azonnal-szornyethalt";
    private const string TELEX_ARTICLE = "https://telex.hu/belfold/2024/03/31/orosz-propaganda-magyarok-titko-szolgalatok";
    private const string RTL_ARTICLE = "https://rtl.hu/hazon-kivul/2024/03/31/duna-terasz-grande-lakopark-velemenyek-ingatlan";
    private const string INDEX_ARTICLE = "https://index.hu/kultur/2024/03/29/rakay-philip-film-most-vagy-soha-aranybulla-csincsi-zoltan-producer-imdb-internet-szavazas/?token=64ceb0a65e4a13ddfb371b1946da0f8a";
    private const string MERCE_ARTICLE = "https://merce.hu/2024/03/31/vita-a-tekintelyelvuseg-uj-formairol-es-tortenelmi-elozmenyeikrol/";
    private const string ORIGO_ARCHIVE = "https://www.origo.hu/hirarchivum/2024/03/11";
    private const string RIPOST_ARCHIVE = "https://ripost.hu/202404_sitemap.xml";
    private const string MANDINER_ARCHIVE = "https://mandiner.hu/202404_sitemap.xml";
    private const string METROPOL_ARCHIVE = "https://metropol.hu/202404_sitemap.xml";
    private const string MAGYARNEMZET_ARCHIVE = "https://magyarnemzet.hu/202404_sitemap.xml";
    private const string PESTISRACOK_ARCHIVE = "https://pestisracok.hu/2024/03/11";
    private const string MAGYARJELEN_ARCHIVE_DATE = "https://magyarjelen.hu/2024/03/11";
    private const string MAGYARJELEN_ARCHIVE_PAGE = "https://magyarjelen.hu/2024/03/11/page/2/";
    private const string KURUCZINFO_ARCHIVE = "https://kuruc.info/to/1/20/";
    private const string ALFAHIR_ARCHIVE = "https://alfahir.hu/hirek/oldalak/1";
    private const string HUSZONNEGY_ARCHIVE = "https://24.hu/2024/03/11";
    private const string NEGYNEGYNEGY_ARCHIVE = "https://444.hu/2024/03/11";
    private const string HVG_ARCHIVE = "https://hvg.hu/frisshirek/2024.03.11";
    private const string TELEX_ARCHIVE = "https://telex.hu/legfrissebb?oldal=1";
    private const string RTL_ARCHIVE = "https://rtl.hu/legfrissebb?oldal=1";
    private const string INDEX_ARCHIVE = "https://index.hu/sitemap/cikkek_200302.xml";
    private const string MERCE_ARCHIVE = "https://merce.hu/2024/3/11";
    private IContentParserFactory ContentParserFactory = new ContentParserFactory();
    private IPaginatorFactory PaginatorFactory = new PaginatorFactory();
    private ILinkParserFactory LinkParserFactory = new LinkParserFactory();
    private HttpClient webClient = new();
    public ScrapingJobFactory ScrapingJobFactory = new();

    [OneTimeSetUp]
    public void Setup()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    //TODO: use local html files
    [TestCase(NewsSites.Origo, ORIGO_ARTICLE, @".\Expected\origo.txt")]
    [TestCase(NewsSites.Ripost, RIPOST_ARTICLE, @".\Expected\ripost.txt")]
    [TestCase(NewsSites.Mandiner, MANDINER_ARTICLE, @".\Expected\mandiner.txt")]
    [TestCase(NewsSites.Metropol, METROPOL_ARTICLE, @".\Expected\metropol.txt")]
    [TestCase(NewsSites.MagyarNemzet, MAGYARNEMZET_ARTICLE, @".\Expected\magyarnemzet.txt")]
    [TestCase(NewsSites.PestiSracok, PESTISRACOK_ARTICLE, @".\Expected\pestisracok.txt")]
    [TestCase(NewsSites.MagyarJelen, MAGYARJELEN_ARTICLE, @".\Expected\magyarjelen.txt")]
    [TestCase(NewsSites.Kuruczinfo, KURUCINFO_ARTICLE, @".\Expected\kuruczinfo.txt")]//TODO: fix encoding problem when using local html for kuruczinfo
    [TestCase(NewsSites.Alfahir, ALFAHIR_ARTICLE, @".\Expected\alfahir.txt")]
    [TestCase(NewsSites.Huszonnegy, HUSZONNEGY_ARTICLE, @".\Expected\huszonnegy.txt")]//TODO: Add test where there is lead to the article
    [TestCase(NewsSites.NegyNegyNegy, NEGYNEGYNEGY_ARTICLE, @".\Expected\negynegynegy.txt")]
    [TestCase(NewsSites.HVG, HVG_ARTICLE, @".\Expected\hvg.txt")]
    [TestCase(NewsSites.Telex, TELEX_ARTICLE, @".\Expected\telex.txt")]
    [TestCase(NewsSites.RTL, RTL_ARTICLE, @".\Expected\rtl.txt")]
    [TestCase(NewsSites.Index, INDEX_ARTICLE, @".\Expected\index.txt")]
    [TestCase(NewsSites.Merce, MERCE_ARTICLE, @".\Expected\merce.txt")]
    public async Task ContentParserTest(NewsSites source, string siteUrl, string expectedPath)
    {
        IContentParser contentParser = ContentParserFactory.GetContentParser(source);
        Assert.That(File.Exists(expectedPath), "Result txt missing");
        var document = await webClient.GetStringAsync(siteUrl);
        await webClient.GetStringAsync(siteUrl);
        var result = await contentParser.ParseContentAsync(document);
        Assert.That(result.Text, Is.EqualTo(File.ReadAllText(expectedPath)));
    }
    [TestCase(NewsSites.Origo, ORIGO_ARCHIVE, "https://www.origo.hu/hirarchivum/2024/03/10")]
    [TestCase(NewsSites.Ripost, RIPOST_ARCHIVE, "https://ripost.hu/202403_sitemap.xml")]
    [TestCase(NewsSites.Mandiner, MANDINER_ARCHIVE, "https://mandiner.hu/202403_sitemap.xml")]
    [TestCase(NewsSites.Metropol, METROPOL_ARCHIVE, "https://metropol.hu/202403_sitemap.xml")]
    [TestCase(NewsSites.MagyarNemzet, MAGYARNEMZET_ARCHIVE, "https://magyarnemzet.hu/202403_sitemap.xml")]
    [TestCase(NewsSites.PestiSracok, PESTISRACOK_ARCHIVE, "https://www.pestisracok.hu/2024/03/10")]
    [TestCase(NewsSites.MagyarJelen, MAGYARJELEN_ARCHIVE_DATE, "https://magyarjelen.hu/2024/03/11/page/2/")]
    [TestCase(NewsSites.MagyarJelen, MAGYARJELEN_ARCHIVE_PAGE, "https://magyarjelen.hu/2024/03/10")]
    [TestCase(NewsSites.Kuruczinfo, KURUCZINFO_ARCHIVE, "https://kuruc.info/to/1/40/")]//TODO: fix encoding problem when using local html for kuruczinfo
    [TestCase(NewsSites.Alfahir, ALFAHIR_ARCHIVE, "https://alfahir.hu/hirek/oldalak/2")]
    [TestCase(NewsSites.Huszonnegy, HUSZONNEGY_ARCHIVE, "https://24.hu/2024/03/10")]//TODO: Add test where there is lead to the article
    [TestCase(NewsSites.NegyNegyNegy, NEGYNEGYNEGY_ARCHIVE, "https://444.hu/2024/03/10")]
    [TestCase(NewsSites.HVG, HVG_ARCHIVE, "http://hvg.hu/frisshirek/2024.03.10")]
    [TestCase(NewsSites.Telex, TELEX_ARCHIVE, "https://telex.hu/legfrissebb?oldal=2")]
    [TestCase(NewsSites.RTL, RTL_ARCHIVE, "https://rtl.hu/legfrissebb?oldal=2")]
    [TestCase(NewsSites.Index, INDEX_ARCHIVE, "https://index.hu/sitemap/cikkek_200301.xml")]
    [TestCase(NewsSites.Merce, MERCE_ARCHIVE, "https://merce.hu/2024/03/10")]
    public async Task ArchivePaginatorTest(NewsSites source, string siteUrl, string expectUrl)
    {
        IPaginator paginator = PaginatorFactory.GetPaginator(source);
        var document = await webClient.GetStringAsync(siteUrl);
        var result = await paginator.GetNextPageAsync(siteUrl, document);
        Assert.That(result.Url, Is.EqualTo(expectUrl));
    }

    [TestCase(NewsSites.Origo, ORIGO_ARCHIVE, 101)]
    [TestCase(NewsSites.Ripost, RIPOST_ARCHIVE, 2284)]
    [TestCase(NewsSites.Mandiner, MANDINER_ARCHIVE, 3153)]
    [TestCase(NewsSites.Metropol, METROPOL_ARCHIVE, 1960)]
    [TestCase(NewsSites.MagyarNemzet, MAGYARNEMZET_ARCHIVE, 4165)]
    [TestCase(NewsSites.PestiSracok, PESTISRACOK_ARCHIVE, 44)]
    [TestCase(NewsSites.MagyarJelen, MAGYARJELEN_ARCHIVE_DATE, 11)]
    [TestCase(NewsSites.MagyarJelen, MAGYARJELEN_ARCHIVE_PAGE, 3)]
    [TestCase(NewsSites.Kuruczinfo, KURUCZINFO_ARCHIVE, 21)]//TODO: fix encoding problem when using local html for kuruczinfo
    [TestCase(NewsSites.Alfahir, ALFAHIR_ARCHIVE, 10)]
    [TestCase(NewsSites.Huszonnegy, HUSZONNEGY_ARCHIVE, 25)]//TODO: Add test where there is lead to the article
    [TestCase(NewsSites.NegyNegyNegy, NEGYNEGYNEGY_ARCHIVE, 56)]
    [TestCase(NewsSites.HVG, HVG_ARCHIVE, 41)]
    [TestCase(NewsSites.Telex, TELEX_ARCHIVE, 12)]
    [TestCase(NewsSites.RTL, RTL_ARCHIVE, 51)]
    [TestCase(NewsSites.Index, INDEX_ARCHIVE, 2462)]
    [TestCase(NewsSites.Merce, MERCE_ARCHIVE, 5)]
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