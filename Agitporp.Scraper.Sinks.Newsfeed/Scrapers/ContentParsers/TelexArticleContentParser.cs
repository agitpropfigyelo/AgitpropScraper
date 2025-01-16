using System;
using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class TelexArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//*[@id='cikk-content']/div[1]/div[2]/div[2]/p/span" };
    protected override List<string> TitleXPaths => new List<string> { "//div[@class='title-section__top']" };
    protected override List<string> LeadXPaths => new List<string> { };
    protected override List<string> ArticleXPaths => new List<string> { "//div[contains(@class, 'article-html-content')]" };
    protected override NewsSites SourceSite => NewsSites.Telex;
}
