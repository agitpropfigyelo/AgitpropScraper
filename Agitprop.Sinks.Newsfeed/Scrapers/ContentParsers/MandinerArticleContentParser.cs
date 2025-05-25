namespace Agitprop.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class MandinerArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//meta[@property='article:published_time']" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='article-page-title']" };
    protected override List<string> LeadXPaths => new List<string> { "//p[@class='article-page-lead']" };
    protected override List<string> ArticleXPaths => new List<string> { "//man-wysiwyg-box" };
    protected override NewsSites SourceSite => NewsSites.Mandiner;
}
