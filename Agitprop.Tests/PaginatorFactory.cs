using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure;
using Agitprop.Scrapers;

namespace NewsArticleScraper.Tests;

internal class PaginatorFactory : IPaginatorFactory
{
    public IPaginator GetPaginator(NewsSites source)
    {
        return source switch
        {
            NewsSites.Origo => new Agitprop.Scrapers.Origo.ArchivePaginator(),
            NewsSites.Ripost => new Agitprop.Scrapers.Ripost.ArchivePaginator(),
            NewsSites.Mandiner => new Agitprop.Scrapers.Mandiner.ArchivePaginator(),
            NewsSites.Metropol => new Agitprop.Scrapers.Metropol.ArchivePaginator(),
            NewsSites.MagyarNemzet => new Agitprop.Scrapers.Magyarnemzet.ArchivePaginator(),
            NewsSites.PestiSracok => new Agitprop.Scrapers.Pestisracok.ArchivePaginator(),
            NewsSites.MagyarJelen => new Agitprop.Scrapers.Magyarjelen.ArchivePaginator(),
            NewsSites.Kuruczinfo => new Agitprop.Scrapers.Kuruczinfo.ArchivePaginator(),
            NewsSites.Alfahir => new Agitprop.Scrapers.Alfahir.ArchivePaginator(),
            NewsSites.Huszonnegy => new Agitprop.Scrapers.Huszonnegy.ArchivePaginator(),
            NewsSites.NegyNegyNegy => new Agitprop.Scrapers.Negynegynegy.ArchivePaginator(),
            NewsSites.HVG => new Agitprop.Scrapers.Hvg.ArchivePaginator(),
            NewsSites.Telex => new Agitprop.Scrapers.Telex.ArchivePaginator(),
            NewsSites.RTL => new Agitprop.Scrapers.Rtl.ArchivePaginator(),
            NewsSites.Index => new Agitprop.Scrapers.Index.ArchivePaginator(),
            NewsSites.Merce => new Agitprop.Scrapers.Merce.ArchivePaginator(),
            _ => throw new ArgumentException($"Not supported news source: {source}"),
        };
    }
}
