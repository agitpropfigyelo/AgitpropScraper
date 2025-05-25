using Agitprop.Core.Enums;
using Agitprop.Sinks.Newsfeed.Scrapers.ContentParsers;

namespace Agitprop.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class MagyarNemzetArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//meta[@property='article:published_time']" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='title']" };
    protected override List<string> LeadXPaths => new List<string> { "//h2[@class='lead']" };
    protected override List<string> ArticleXPaths => new List<string> { "//app-article-text" };
    protected override NewsSites SourceSite => NewsSites.MagyarNemzet;
}
