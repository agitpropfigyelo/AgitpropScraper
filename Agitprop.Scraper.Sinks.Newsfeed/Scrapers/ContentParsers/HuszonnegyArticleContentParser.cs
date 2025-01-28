using Agitprop.Core.Enums;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class HuszonnegyArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//meta[@property='article:published_time']" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='o-post__title']" };
    protected override List<string> LeadXPaths => new List<string> { "//h1[@class='o-post__lead lead post-lead cf _ce_measure_widget']" };
    protected override List<string> ArticleXPaths => new List<string> { "//div[@class='o-post__body post-body']", "//div[@class='article-body']" };
    protected override NewsSites SourceSite => NewsSites.Huszonnegy;
}
