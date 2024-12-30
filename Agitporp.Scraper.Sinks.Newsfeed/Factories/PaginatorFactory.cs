using Agitprop.Core.Enums;
using Agitprop.Core.Factories;
using Agitprop.Core.Interfaces;

namespace Agitporp.Scraper.Sinks.Newsfeed.Factories;

public class PaginatorFactory : IPaginatorFactory
{
    public IPaginator GetPaginator(NewsSites source)
    {
        return source switch
        {
            NewsSites.Origo => new Origo.ArchivePaginator(),
            NewsSites.Ripost => new Ripost.ArchivePaginator(),
            NewsSites.Mandiner => new Mandiner.ArchivePaginator(),
            NewsSites.Metropol => new Metropol.ArchivePaginator(),
            NewsSites.MagyarNemzet => new Magyarnemzet.ArchivePaginator(),
            NewsSites.PestiSracok => new Pestisracok.ArchivePaginator(),
            NewsSites.MagyarJelen => new Magyarjelen.ArchivePaginator(),
            NewsSites.Kurucinfo => new Kurucinfo.ArchivePaginator(),
            NewsSites.Alfahir => new Alfahir.ArchivePaginator(),
            NewsSites.Huszonnegy => new Huszonnegy.ArchivePaginator(),
            NewsSites.NegyNegyNegy => new Negynegynegy.ArchivePaginator(),
            NewsSites.HVG => new Hvg.ArchivePaginator(),
            NewsSites.Telex => new Telex.ArchivePaginator(),
            NewsSites.RTL => new Rtl.ArchivePaginator(),
            NewsSites.Index => new Index.ArchivePaginator(),
            NewsSites.Merce => new Merce.ArchivePaginator(),
            _ => throw new ArgumentException($"Not supported news source: {source}"),
        };
    }
}
