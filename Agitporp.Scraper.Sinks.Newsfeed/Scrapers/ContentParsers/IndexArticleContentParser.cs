using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class IndexArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//*[@id='content']/div[4]/div[1]/div/div[1]/div[2]/span" };
    protected override List<string> TitleXPaths => new List<string> { "//div[@class='content-title']" };
    protected override List<string> LeadXPaths => new List<string> { "//div[@class='lead']" };
    protected override List<string> ArticleXPaths => new List<string> { "//div[@class='cikk-torzs']" };
    protected override NewsSites SourceSite => NewsSites.Index;
}
