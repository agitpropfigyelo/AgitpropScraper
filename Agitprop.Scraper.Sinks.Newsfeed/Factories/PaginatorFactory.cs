using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

namespace Agitprop.Scraper.Sinks.Newsfeed.Factories;

internal static class PaginatorFactory
{
    public static IPaginator GetPaginator(NewsSites source)
    {
        return source switch
        {
            NewsSites.Origo => new Scrapers.Origo.ArchivePaginator(),
            NewsSites.Ripost => new Scrapers.Ripost.ArchivePaginator(),
            NewsSites.Mandiner => new Scrapers.Mandiner.ArchivePaginator(),
            NewsSites.Metropol => new Scrapers.Metropol.ArchivePaginator(),
            NewsSites.MagyarNemzet => new Scrapers.Magyarnemzet.ArchivePaginator(),
            NewsSites.PestiSracok => new Scrapers.Pestisracok.ArchivePaginator(),
            NewsSites.MagyarJelen => new Scrapers.Magyarjelen.ArchivePaginator(),
            NewsSites.Kurucinfo => new Scrapers.Kurucinfo.ArchivePaginator(),
            NewsSites.Alfahir => new Scrapers.Alfahir.ArchivePaginator(),
            NewsSites.Huszonnegy => new Scrapers.Huszonnegy.ArchivePaginator(),
            NewsSites.NegyNegyNegy => new Scrapers.Negynegynegy.ArchivePaginator(),
            NewsSites.HVG => new Scrapers.Hvg.ArchivePaginator(),
            NewsSites.Telex => new Scrapers.Telex.ArchivePaginator(),
            NewsSites.RTL => new Scrapers.Rtl.ArchivePaginator(),
            NewsSites.Index => new Scrapers.Index.ArchivePaginator(),
            NewsSites.Merce => new Scrapers.Merce.ArchivePaginator(),
            _ => throw new ArgumentException($"Not supported news source: {source}"),
        };
    }
}
