namespace Agitprop.Scraper.Sinks.Newsfeed_Test;

using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Agitprop.Core;
using Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

public class ContentParserTests
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
        var json = File.ReadAllText("TestData/Alfahir/testCases.json");
        return JsonSerializer.Deserialize<List<TestCase>>(json);
    }

    public class TestCase
    {
        public string HtmlPath { get; set; }
        public ContentParserResult ExpectedContent { get; set; }
    }
}