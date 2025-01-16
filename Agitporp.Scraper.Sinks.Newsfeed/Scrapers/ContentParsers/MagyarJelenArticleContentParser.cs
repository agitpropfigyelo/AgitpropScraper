using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class MagyarJelenArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "/html/body/div[1]/div[5]/div[1]/div/div/span[2]/time" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='is-title post-title']" };
    protected override List<string> LeadXPaths => new List<string> { };
    protected override List<string> ArticleXPaths => new List<string> { "//div[@class='post-content cf entry-content content-spacious']" };
    protected override NewsSites SourceSite => NewsSites.MagyarJelen;
}
