using System;
using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class NegynegynegyArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "/html/body/div[1]/div[1]/div[3]/div[4]/div/div[2]/div[2]" };
    protected override List<string> TitleXPaths => new List<string> { "/html/body/div[1]/div[1]/div[3]/div[3]/h1" };
    protected override List<string> LeadXPaths => new List<string> { };
    protected override List<string> ArticleXPaths => new List<string> { "/html/body/div[1]/div[1]/div[3]/div[5]/div/div[2]//p" };
    protected override NewsSites SourceSite => NewsSites.NegyNegyNegy;
}
