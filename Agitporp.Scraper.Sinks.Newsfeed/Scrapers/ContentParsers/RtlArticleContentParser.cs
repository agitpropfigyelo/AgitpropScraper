using System;
using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal class RtlArticleContentParser : BaseArticleContentParser
{
    protected override List<string> DateXPaths => new List<string> { "//*[@id='app']/main/section/div/div[2]/div[1]/div[2]/p" };
    protected override List<string> TitleXPaths => new List<string> { "//h1[@class='page-layout__title']" };
    protected override List<string> LeadXPaths => new List<string> { "//div[contains(@class, 'static-page__content static-page__content--lead')]" };
    protected override List<string> ArticleXPaths => new List<string> { "//div[contains(@class, 'static-page__content static-page__content--body')]" };
    protected override NewsSites SourceSite => NewsSites.RTL;
}
