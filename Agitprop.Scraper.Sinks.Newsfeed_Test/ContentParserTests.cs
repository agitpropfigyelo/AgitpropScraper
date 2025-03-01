using Agitprop.Core.Enums;
using Agitprop.Scraper.Sinks.Newsfeed.Factories;

namespace Agitprop.Scraper.Sinks.Newsfeed_Test;
public partial class ContentParserTests
{
    [TestCase(NewsSites.Alfahir)]
    [TestCase(NewsSites.HVG)]
    [TestCase(NewsSites.Index)]
    //[TestCase("TestData/kurucinfo/testCases.json")]
    [TestCase(NewsSites.MagyarJelen)]
    [TestCase(NewsSites.MagyarNemzet)]
    [TestCase(NewsSites.Mandiner)]
    [TestCase(NewsSites.Merce)]
    [TestCase(NewsSites.Metropol)]
    [TestCase(NewsSites.Origo)]
    [TestCase(NewsSites.PestiSracok)]
    [TestCase(NewsSites.Ripost)]
    [TestCase(NewsSites.RTL)]
    [TestCase(NewsSites.Telex)]
    [TestCase(NewsSites.Huszonnegy)]
    [TestCase(NewsSites.NegyNegyNegy)]
    public void ContentParserTest(NewsSites site)
    {
        var scraper = ContentParserFactory.GetContentParser(site);
        foreach (var testCase in TestCaseFactory.GetContentParserTestCases(site))
        {
            var htmlContent = File.ReadAllText(testCase.HtmlPath);
            var result = scraper.ParseContentAsync(htmlContent).Result;
            Assert.Multiple(() =>
            {

                Assert.That(result.SourceSite, Is.EqualTo(testCase.ExpectedContent.SourceSite));
                Assert.That(result.PublishDate, Is.EqualTo(testCase.ExpectedContent.PublishDate));
                Assert.That(result.Text, Is.EqualTo(testCase.ExpectedContent.Text));
            });
        }
    }
}