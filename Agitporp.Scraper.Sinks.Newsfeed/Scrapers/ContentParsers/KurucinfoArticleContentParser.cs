using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class KurucinfoArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//*[@id='cikkcontent']/div/p[1]/span[1]" };
    protected override List<string> TitleXPaths => new List<string> { "//div[@class='focikkheader']" };
    protected override List<string> LeadXPaths => new List<string> { };
    protected override List<string> ArticleXPaths => new List<string> { "//div[contains(@class, 'cikktext')]" };
    protected override NewsSites SourceSite => NewsSites.Kurucinfo;
}
