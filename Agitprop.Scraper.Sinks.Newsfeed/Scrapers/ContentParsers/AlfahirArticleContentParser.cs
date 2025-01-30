using Agitprop.Core.Enums;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class AlfahirArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//meta[@name='og:article:published_time']/@content" };
    protected override List<string> TitleXPaths => new List<string> { "/html/body/main/div/div/article/h1" };
    protected override List<string> LeadXPaths => new List<string> { "/html/body/main/div/div/article/p" };
    protected override List<string> ArticleXPaths => new List<string> { "/html/body/main/div/div/article/div[5]/div/div[1]/*[not(self::div)]" };
    protected override NewsSites SourceSite => NewsSites.Alfahir;
}
