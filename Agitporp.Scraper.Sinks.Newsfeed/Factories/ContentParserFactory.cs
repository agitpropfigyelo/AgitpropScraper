using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

namespace Agitporp.Scraper.Sinks.Newsfeed.Factories;

internal static class ContentParserFactory
{
    public static IContentParser GetContentParser(NewsSites siteIn)
    {
        return siteIn switch
        {
            NewsSites.Origo => new Scrapers.Origo.ArticleContentParser(),
            NewsSites.Ripost => new Scrapers.Ripost.ArticleContentParser(),
            NewsSites.Mandiner => new Scrapers.Mandiner.ArticleContentParser(),
            NewsSites.Metropol => new Scrapers.Metropol.ArticleContentParser(),
            NewsSites.MagyarNemzet => new Scrapers.Magyarnemzet.ArticleContentParser(),
            NewsSites.PestiSracok => new Scrapers.Pestisracok.ArticleContentParser(),
            NewsSites.MagyarJelen => new Scrapers.Magyarjelen.ArticleContentParser(),
            NewsSites.Kurucinfo => new Scrapers.Kurucinfo.ArticleContentParser(),
            NewsSites.Alfahir => new Scrapers.Alfahir.ArticleContentParser(),
            NewsSites.Huszonnegy => new Scrapers.Huszonnegy.ArticleContentParser(),
            NewsSites.NegyNegyNegy => new Scrapers.Negynegynegy.ArticleContentParser(),
            NewsSites.HVG => new Scrapers.Hvg.ArticleContentParser(),
            NewsSites.Telex => new Scrapers.Telex.ArticleContentParser(),
            NewsSites.RTL => new Scrapers.Rtl.ArticleContentParser(),
            NewsSites.Index => new Scrapers.Index.ArticleContentParser(),
            NewsSites.Merce => new Scrapers.Merce.ArticleContentParser(),
            _ => throw new ArgumentException($"Not supported news source: {siteIn}"),
        };
    }
}
