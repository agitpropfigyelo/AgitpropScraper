using Agitprop.Core.Enums;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class AlfahirArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "/html/body/main/div/div/article/div[1]/div/span[1]" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='article-title']" };
    protected override List<string> LeadXPaths => new List<string> { "//p[@class='article-lead']" };
    protected override List<string> ArticleXPaths => new List<string> { "//div[contains(@class, 'article-content')][2]", "//div[@class='article-body']" };
    protected override NewsSites SourceSite => NewsSites.Alfahir;
}
