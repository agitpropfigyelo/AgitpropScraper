using System;
using Agitprop.Core.Enums;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class PestiSracokArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//*[@id='left-content']/div[2]/span/time" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[contains(@class, 'story-title entry-title')]" };
    protected override List<string> LeadXPaths => new List<string> { };
    protected override List<string> ArticleXPaths => new List<string> { "//div[contains(@class, 'wprt-container')]" };
    protected override NewsSites SourceSite => NewsSites.PestiSracok;
}
