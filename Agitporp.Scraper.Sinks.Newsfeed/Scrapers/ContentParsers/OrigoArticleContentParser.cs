using System;
using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class OrigoArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "/html/body/app-root/app-base/div[2]/app-article-page/section/div[1]/div/app-article-header/article/app-article-meta/div/div[1]/div", "/html/body/app-root/app-base/div[2]/app-sport-article-page/section/div/div[1]/div/app-sport-article-header/article/app-sport-article-author/div/div/div/div" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='article-title']" };
    protected override List<string> LeadXPaths => new List<string> { "//div[@class='article-lead']" };
    protected override List<string> ArticleXPaths => new List<string> { "//origo-wysiwyg-box" };
    protected override NewsSites SourceSite => NewsSites.Origo;
}
