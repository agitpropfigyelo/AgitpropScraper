using System;
using Agitprop.Core.Enums;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class MerceArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "/html/body/div[4]/div/div/main/div/div[2]/div/article/div/div[1]/div[1]/div[2]/div[2]/time" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='entry-title']" };
    protected override List<string> LeadXPaths => new List<string> { };
    protected override List<string> ArticleXPaths => new List<string> { "//div[contains(@class, 'entry-content')]" };
    protected override NewsSites SourceSite => NewsSites.Merce;
}
