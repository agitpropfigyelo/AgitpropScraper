using System;
using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class RipostArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "/html/body/app-root/app-base/div/app-article-page/section/div[1]/div/app-article-page-head/div/div/div[2]/div[1]" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='title']" };
    protected override List<string> LeadXPaths => new List<string> { "//div[@class='article-page-lead']" };
    protected override List<string> ArticleXPaths => new List<string> { "//app-wysiwyg-box" };
    protected override NewsSites SourceSite => NewsSites.Ripost;
}
