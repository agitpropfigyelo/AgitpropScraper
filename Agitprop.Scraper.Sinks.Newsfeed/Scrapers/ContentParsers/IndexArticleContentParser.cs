using Agitprop.Core.Enums;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class IndexArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "/html/head/meta[27]" };
    protected override List<string> TitleXPaths => new List<string> { "//div[@class='content-title']" };
    protected override List<string> LeadXPaths => new List<string> { "//div[@class='lead']" };
    protected override List<string> ArticleXPaths => new List<string> { "//div[@class='cikk-torzs']" };
    protected override NewsSites SourceSite => NewsSites.Index;
}
