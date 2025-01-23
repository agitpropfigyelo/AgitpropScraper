using Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

using Agitprop.Core.Enums;

internal class MagyarNemzetArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "/html/body/app-root/app-base/div[2]/div/app-slug-route-handler/app-article/section/div/div[1]/app-article-header/div/div[3]/div[1]/div/span[2]" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='title']" };
    protected override List<string> LeadXPaths => new List<string> { "//h2[@class='lead']" };
    protected override List<string> ArticleXPaths => new List<string> { "//app-article-text" };
    protected override NewsSites SourceSite => NewsSites.MagyarNemzet;
}
