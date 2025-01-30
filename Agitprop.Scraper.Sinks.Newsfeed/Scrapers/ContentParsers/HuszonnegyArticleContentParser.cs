using Agitprop.Core.Enums;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class HuszonnegyArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//meta[@property='article:published_time']" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='o-post__title']" };
    protected override List<string> LeadXPaths => new List<string> { "//h1[@class='o-post__lead lead post-lead cf _ce_measure_widget']" };
    protected override List<string> ArticleXPaths => new List<string> { "//*[@id='content']/div/div[1]/div[2]/div[1]/div[2]/div[6]/div[1]" };
    protected override NewsSites SourceSite => NewsSites.Huszonnegy;
}
