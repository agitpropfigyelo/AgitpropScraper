namespace Agitprop.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class OrigoArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//meta[@property='article:published_time']" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='article-title']" };
    protected override List<string> LeadXPaths => new List<string> { "//div[@class='article-lead']" };
    protected override List<string> ArticleXPaths => new List<string> { "//origo-wysiwyg-box" };
    protected override NewsSites SourceSite => NewsSites.Origo;
}
