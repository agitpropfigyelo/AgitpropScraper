namespace Agitprop.Scraper.Sinks.Newsfeed_Test;

using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Agitprop.Core;
using Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;
using Agitprop.Core.Enums;

public partial class ContentParserTests
{
    [SetUp]
    public void Setup()
    {
    }

    [TestCaseSource(nameof(GetTestCases))]
    public void Alfahir(TestCase testCase)
    {
        var scraper = new AlfahirArticleContentParser();
        var htmlContent = File.ReadAllText(testCase.HtmlPath);
        var result = scraper.ParseContentAsync(htmlContent).Result;
        Assert.That(result, Is.EqualTo(testCase.ExpectedContent));
    }

    public static IEnumerable<TestCase> GetTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/alfahir/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.Alfahir,
                    PublishDate = DateTime.Parse("2025-01-08T16:01:05Z"),
                    Text = "Ezúttal egy 85 éves bácsit vertek félholtra az otthonában Egyre gyakoribb, hogy idős emberekre rontanak rá a házukban."
                }
            },
        };
    }
}