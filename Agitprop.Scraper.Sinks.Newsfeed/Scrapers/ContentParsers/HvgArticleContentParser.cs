using Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

using Agitprop.Core.Enums;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers;

internal class HvgArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//meta[@property='article:published_time']" };
    protected override List<string> TitleXPaths => new List<string> { "//div[@class='article-title article-title']" };
    protected override List<string> LeadXPaths => new List<string> { "//p[contains(@class, 'article-lead entry-summary')]" };
    protected override List<string> ArticleXPaths => new List<string> { "//div[contains(@class, 'article-content entry-content')]", "//div[@class='article-body']" };
    protected override NewsSites SourceSite => NewsSites.HVG;
}
