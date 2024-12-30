using Agitprop.Core.Enums;
using Agitprop.Core.Factories;
using Agitprop.Core.Interfaces;

namespace Agitporp.Scraper.Sinks.Newsfeed.Factories;

public class ContentParserFactory : IContentParserFactory
{
    public ContentParserFactory()
    {

    }
    public IContentParser GetContentParser(NewsSites siteIn)
    {
        return siteIn switch
        {
            NewsSites.Origo => new Origo.ArticleContentParser(),
            NewsSites.Ripost => new Ripost.ArticleContentParser(),
            NewsSites.Mandiner => new Mandiner.ArticleContentParser(),
            NewsSites.Metropol => new Metropol.ArticleContentParser(),
            NewsSites.MagyarNemzet => new Magyarnemzet.ArticleContentParser(),
            NewsSites.PestiSracok => new Pestisracok.ArticleContentParser(),
            NewsSites.MagyarJelen => new Magyarjelen.ArticleContentParser(),
            NewsSites.Kurucinfo => new Kurucinfo.ArticleContentParser(),
            NewsSites.Alfahir => new Alfahir.ArticleContentParser(),
            NewsSites.Huszonnegy => new Huszonnegy.ArticleContentParser(),
            NewsSites.NegyNegyNegy => new Negynegynegy.ArticleContentParser(),
            NewsSites.HVG => new Hvg.ArticleContentParser(),
            NewsSites.Telex => new Telex.ArticleContentParser(),
            NewsSites.RTL => new Rtl.ArticleContentParser(),
            NewsSites.Index => new Index.ArticleContentParser(),
            NewsSites.Merce => new Merce.ArticleContentParser(),
            _ => throw new ArgumentException($"Not supported news source: {siteIn}"),
        };
    }
}
