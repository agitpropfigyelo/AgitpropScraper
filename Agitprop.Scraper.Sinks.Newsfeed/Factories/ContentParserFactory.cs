using Agitprop.Scraper.Sinks.Newsfeed.Scrapers;
using Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

namespace Agitprop.Scraper.Sinks.Newsfeed.Factories;

internal static class ContentParserFactory
{
    public static IContentParser GetContentParser(NewsSites siteIn)
    {
        return siteIn switch
        {
            NewsSites.Origo => new OrigoArticleContentParser(),
            NewsSites.Ripost => new RipostArticleContentParser(),
            NewsSites.Mandiner => new MandinerArticleContentParser(),
            NewsSites.Metropol => new  MetropolArticleContentParser(),
            NewsSites.MagyarNemzet => new  MagyarNemzetArticleContentParser(),
            NewsSites.PestiSracok => new  PestiSracokArticleContentParser(),
            NewsSites.MagyarJelen => new  MagyarJelenArticleContentParser(),
            NewsSites.Kurucinfo => throw new NotImplementedException("Kurucinfo is not currently parsable"), //new  KurucinfoArticleContentParser(),
            NewsSites.Alfahir => new  AlfahirArticleContentParser(),
            NewsSites.HuszonnegyHu => new  HuszonnegyArticleContentParser(),
            NewsSites.NegyNegyNegy => new  NegynegynegyArticleContentParser(),
            NewsSites.HVG => new  HvgArticleContentParser(),
            NewsSites.Telex => new  TelexArticleContentParser(),
            NewsSites.RTL => new  RtlArticleContentParser(),
            NewsSites.Index => new  IndexArticleContentParser(),
            NewsSites.Merce => new  MerceArticleContentParser(),
            _ => throw new ArgumentException($"Not supported news source: {siteIn}"),
        };
    }
}
