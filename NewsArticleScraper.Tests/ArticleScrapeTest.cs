using System.Globalization;
using HtmlAgilityPack;
using NewsArticleScraper.Core;
using NewsArticleScraper.Scrapers;

namespace NewsArticleScraper.Tests;

[Parallelizable(scope: ParallelScope.All)]
public class ArticleScrapeTest
{
    private IScraperFactory factory;
    private HtmlWeb webClient;
    [OneTimeSetUp]
    public void OneTimeSetUpSetUp()
    {
        factory = new NewsSiteScraperFactory();
        webClient = new HtmlWeb() { AutoDetectEncoding = true };
    }

    //TODO: use local html files
    [TestCase(NewsSites.Origo, "https://www.origo.hu/itthon/2024/03/magyar-peter-az-uj-marki-zay-csak-meg-agresszivabban-baloldali", @".\Expected\origo.txt")]
    [TestCase(NewsSites.Ripost, "https://ripost.hu/insider/2024/03/orban-viktor-levelet-kuldott-vlagyimir-putyinnak", @".\Expected\ripost.txt")]
    [TestCase(NewsSites.Mandiner, "https://mandiner.hu/sport/2024/03/a-halalos-betegseggel-kuzdo-sven-goran-eriksson-leult-a-liverpool-kispadjara-koszontese-elkepeszto-volt-video", @".\Expected\mandiner.txt")]
    [TestCase(NewsSites.Metropol, "https://metropol.hu/aktualis/2024/03/kitalalt-magyar-peterrol-egy-csaladi-barat-pszichiaterre-volt-szukseg", @".\Expected\metropol.txt")]
    [TestCase(NewsSites.MagyarNemzet, "https://magyarnemzet.hu/belfold/2024/03/megszolalt-magyar-peter-csaladi-baratja-varga-judit", @".\Expected\magyarnemzet.txt")]
    [TestCase(NewsSites.PestiSracok, "https://pestisracok.hu/lepeselonyben-a-globalista-elnokjelolt-szlovakiaban-de-csak-ket-het-mulva-lesz-dontes/", @".\Expected\pestisracok.txt")]
    [TestCase(NewsSites.MagyarJelen, "https://magyarjelen.hu/kielezett-kuzdelmet-hozott-a-felvideki-allamfovalasztas-elso-forduloja/", @".\Expected\magyarjelen.txt")]
    [TestCase(NewsSites.Kuruczinfo, "https://kuruc.info/r/6/271859/", @".\Expected\kuruczinfo.txt")]//TODO: fix encoding problem when using local html for kuruczinfo
    [TestCase(NewsSites.Alfahir, "https://alfahir.hu/hirek/ismet-csokkentette-az-alapkamatot-a-matolcsy-vezette-jegybank", @".\Expected\alfahir.txt")]
    [TestCase(NewsSites.Huszonnegy, "https://24.hu/belfold/2024/03/29/titkos-megfigyeles-varga-judit-tuzson-bence-pegazus/", @".\Expected\huszonnegy.txt")]//TODO: Add test where there is lead to the article
    [TestCase(NewsSites.NegyNegyNegy, "https://444.hu/2024/03/30/jockey-es-samantha-varja-most-az-erkezoket-varga-judit-balatonhenyei-hazanak-kapujaban", @".\Expected\negynegynegy.txt")]
    [TestCase(NewsSites.HVG, "https://hvg.hu/cegauto/20240331_egy-200-kilos-szarvas-zuhant-egy-autora-kesztolcnel-az-egyik-utas-azonnal-szornyethalt", @".\Expected\hvg.txt")]
    [TestCase(NewsSites.Telex, "https://telex.hu/belfold/2024/03/31/orosz-propaganda-magyarok-titko-szolgalatok", @".\Expected\telex.txt")]
    [TestCase(NewsSites.RTL, "https://rtl.hu/hazon-kivul/2024/03/31/duna-terasz-grande-lakopark-velemenyek-ingatlan", @".\Expected\rtl.txt")]
    [TestCase(NewsSites.Index, "https://index.hu/kultur/2024/03/29/rakay-philip-film-most-vagy-soha-aranybulla-csincsi-zoltan-producer-imdb-internet-szavazas/?token=64ceb0a65e4a13ddfb371b1946da0f8a", @".\Expected\index.txt")]
    [TestCase(NewsSites.Merce, "https://merce.hu/2024/03/31/vita-a-tekintelyelvuseg-uj-formairol-es-tortenelmi-elozmenyeikrol/", @".\Expected\merce.txt")]
    [Parallelizable(scope: ParallelScope.All)]
    public void ScrapeArticle(NewsSites source, string siteUrl, string expectedPath)
    {
        INewsSiteScraper scraper = factory.GetScraperForSite(source);
        Assert.That(File.Exists(expectedPath), "Result txt missing");
        HtmlDocument document = webClient.Load(siteUrl);
        string result = scraper.GetArticleContent(document);
        Assert.That(result, Is.EqualTo(File.ReadAllText(expectedPath)));
    }

    [TestCase(NewsSites.Origo, "2024.03.11", 100)]
    [TestCase(NewsSites.Ripost, "2024.03.11", 76)]
    [TestCase(NewsSites.Mandiner, "2024.03.11", 100)]
    [TestCase(NewsSites.Metropol, "2024.03.11", 70)]
    [TestCase(NewsSites.MagyarNemzet, "2024.03.11", 147)]
    [TestCase(NewsSites.PestiSracok, "2024.03.11", 43)]
    [TestCase(NewsSites.MagyarJelen, "2024.03.11", 12)]
    [TestCase(NewsSites.Kuruczinfo, "2024.03.11", 34)]
    [TestCase(NewsSites.Alfahir, "2024.03.11", 11)]
    [TestCase(NewsSites.Huszonnegy, "2024.03.24", 24)]
    [TestCase(NewsSites.NegyNegyNegy, "2024.03.11", 76)]
    [TestCase(NewsSites.HVG, "2024.03.11", 76)]
    [TestCase(NewsSites.Telex, "2024.03.11", 76)]
    [TestCase(NewsSites.RTL, "2024.03.11", 76)]
    [TestCase(NewsSites.Index, "2024.03.11", 141)]
    [TestCase(NewsSites.Merce, "2024.03.11", 4)]
    [Parallelizable(scope: ParallelScope.All)]
    public async Task ScrapeArchive(NewsSites source, string dateIn, int expectedArticleCount)
    {
        INewsSiteScraper scraper = factory.GetScraperForSite(source);
        var dateArg = DateTime.Parse(dateIn, CultureInfo.InvariantCulture);
        var result = await scraper.GetArticlesForDateAsync(dateArg);
        Assert.That(result, Has.Count.EqualTo(expectedArticleCount));
    }


    [TestCase(NewsSites.Alfahir, "2024.01.11")]
    public void ScrapeArchive_ThrowsInvalidOperationException(NewsSites source, string dateIn)
    {
        INewsSiteScraper scraper = factory.GetScraperForSite(source);
        var dateArg = DateTime.Parse(dateIn, CultureInfo.InvariantCulture);
        Assert.ThrowsAsync<InvalidOperationException>(() => scraper.GetArticlesForDateAsync(dateArg));
    }

}