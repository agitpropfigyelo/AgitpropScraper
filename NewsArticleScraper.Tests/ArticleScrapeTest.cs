using HtmlAgilityPack;
using NewsArticleScraper.Scrapers;

namespace NewsArticleScraper.Tests;

public class ArticleScrapeTest
{
    [SetUp]
    public void SetUp()
    {
    }

    [TestCase(@".\Articles\origo.html",@".\Expected\origo.txt")]
    public void Origo(string sitePath, string expectedPath)
    {
        var scraper = new OrigoScraper();
        Assert.That(File.Exists(sitePath));
        Assert.That(File.Exists(expectedPath));
        HtmlDocument document = new();
        document.LoadHtml(File.ReadAllText(sitePath));
        string result = scraper.GetArticleContent(document);
        File.WriteAllText("res.txt", result);
        Assert.That(result, Is.EqualTo(File.ReadAllText(expectedPath)));
    }

    public void Ripost(string sitePath, string expectedPath){}
    public void Mandiner(string sitePath, string expectedPath){}
    public void Metropol(string sitePath, string expectedPath){}
    public void MagyarNemzet(string sitePath, string expectedPath){}
    public void PestiSracok(string sitePath, string expectedPath){}
    public void MagyarJelen(string sitePath, string expectedPath){}
    public void Kuruczinfo(string sitePath, string expectedPath){}
    public void Alfahir(string sitePath, string expectedPath){}
    public void Huszonnegy(string sitePath, string expectedPath){}
    public void Negynegynegy(string sitePath, string expectedPath){}
    public void Hvg(string sitePath, string expectedPath){}
    public void Telex(string sitePath, string expectedPath){}
    public void Rtl(string sitePath, string expectedPath){}
    public void Index(string sitePath, string expectedPath){}
    public void Merce(string sitePath, string expectedPath){}

}