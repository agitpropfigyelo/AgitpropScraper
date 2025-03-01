using System;
using System.Text.Json;

using Agitprop.Core.Enums;

using static Agitprop.Scraper.Sinks.Newsfeed_Test.ContentParserTests;

namespace Agitprop.Scraper.Sinks.Newsfeed_Test;

public static class TestCaseFactory
{
    internal static IEnumerable<ContentParserTestCase> GetContentParserTestCases(NewsSites site)
    {
        var testCasePath = $"TestData/{site.ToString().ToLower()}/testCases.json";

        var testCases = JsonSerializer.Deserialize<List<ContentParserTestCase>>(File.ReadAllText(testCasePath));
        foreach (var testCase in testCases)
        {
            yield return testCase;
        }
    }

    internal static string GetArchiveParserTestCasePath(NewsSites site)
    {
        return $"TestData/{site.ToString().ToLower()}/archive";
    }
}
