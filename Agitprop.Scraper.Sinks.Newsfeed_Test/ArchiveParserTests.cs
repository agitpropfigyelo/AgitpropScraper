using Agitprop.Core.Enums;
using Agitprop.Scraper.Sinks.Newsfeed.Factories;

using NUnit.Framework;

namespace Agitprop.Scraper.Sinks.Newsfeed_Test;

public class ArchiveParserTests
{
    [SetUp]
    public void Setup()
    {
    }

    //[TestCase(NewsSites.Alfahir, 10)]
    [TestCase(NewsSites.HVG, 157)]
    [TestCase(NewsSites.Index, 3437)]
    //kurucinfo
    [TestCase(NewsSites.MagyarJelen, 8)]
    [TestCase(NewsSites.MagyarNemzet, 4062)]
    [TestCase(NewsSites.Mandiner, 3103)]
    [TestCase(NewsSites.Merce, 3)]
    [TestCase(NewsSites.Metropol, 0)]
    [TestCase(NewsSites.Origo, 0)]
    [TestCase(NewsSites.PestiSracok, 0)]
    [TestCase(NewsSites.Ripost, 0)]
    //[TestCase(NewsSites.RTL, 0)]
    [TestCase(NewsSites.Telex, 0)]
    [TestCase(NewsSites.HuszonnegyHu, 0)]
    [TestCase(NewsSites.NegyNegyNegy, 0)]
    public void ArchiveParserTest(NewsSites siteIn, int expectedCount)
    {
        var parser = ArchiveLinkParserFactory.GetLinkParser(siteIn);
        var htmlContent = File.ReadAllText(TestCaseFactory.GetArchiveParserTestCasePath(siteIn));
        var result = parser.GetLinksAsync("testBaseUrl", htmlContent).Result;
        Assert.That(result, Has.Count.EqualTo(expectedCount));
    }
}
