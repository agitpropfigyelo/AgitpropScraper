using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class HuszonnegyArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//*[@id='content']/div/div[1]/div[1]/div[5]/div[1]/div[2]/span" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='o-post__title']" };
    protected override List<string> LeadXPaths => new List<string> { "//h1[@class='o-post__lead lead post-lead cf _ce_measure_widget']" };
    protected override List<string> ArticleXPaths => new List<string> { "//div[@class='o-post__body post-body']", "//div[@class='article-body']" };
    protected override NewsSites SourceSite => NewsSites.Huszonnegy;
}
