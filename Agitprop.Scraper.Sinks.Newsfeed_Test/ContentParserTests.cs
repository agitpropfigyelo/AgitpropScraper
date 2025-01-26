namespace Agitprop.Scraper.Sinks.Newsfeed_Test;

using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Agitprop.Core;
using Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;
using Agitprop.Core.Enums;
using Agitprop.Scraper.Sinks.Newsfeed.Factories;

public partial class ContentParserTests
{
    [TestCase("TestData/alfahir/testCases.json")]
    [TestCase("TestData/hvg/testCases.json")]
    [TestCase("TestData/index/testCases.json")]
    //[TestCase("TestData/kurucinfo/testCases.json")]
    [TestCase("TestData/magyarjelen/testCases.json")]
    [TestCase("TestData/magyarnemzet/testCases.json")]
    [TestCase("TestData/mandiner/testCases.json")]
    [TestCase("TestData/merce/testCases.json")]
    [TestCase("TestData/metropol/testCases.json")]
    [TestCase("TestData/444/testCases.json")]
    public void ContentParserTest(string testCasePath)
    {
        var testCases = JsonSerializer.Deserialize<List<TestCase>>(File.ReadAllText(testCasePath));
        var scraper = ContentParserFactory.GetContentParser(testCases.First().ExpectedContent.SourceSite);
        foreach (var testCase in testCases)
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