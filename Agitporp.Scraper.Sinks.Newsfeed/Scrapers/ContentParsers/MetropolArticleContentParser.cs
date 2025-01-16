using System;
using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class MetropolArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "/html/body/app-root/app-base/div/app-article-page/section/div[1]/div/metropol-article-header/section/div[5]/div[2]/div" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[contains(@class, 'article-header-title')]" };
    protected override List<string> LeadXPaths => new List<string> { "//p[contains(@class, 'article-header-lead')]" };
    protected override List<string> ArticleXPaths => new List<string> { "//metropol-wysiwyg-box" };
    protected override NewsSites SourceSite => NewsSites.Metropol;
}
