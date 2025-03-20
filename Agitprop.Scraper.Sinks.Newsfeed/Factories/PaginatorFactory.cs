using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ArchivePaginators;

namespace Agitprop.Scraper.Sinks.Newsfeed.Factories;

internal static class PaginatorFactory
{
    public static IPaginator GetPaginator(NewsSites source)
    {
        return source switch
        {
            NewsSites.Origo => new  OrigoArchivePaginator(),
            NewsSites.Ripost => new  RipostArchivePaginator(),
            NewsSites.Mandiner => new  MandinerArchivePaginator(),
            NewsSites.Metropol => new  MetropolArchivePaginator(),
            NewsSites.MagyarNemzet => new  MagyarNemzetArchivePaginator(),
            NewsSites.PestiSracok => new  PestiSracokArchivePaginator(),
            NewsSites.MagyarJelen => new  MagyarJelenArchivePaginator(),
            NewsSites.Kurucinfo => new  KurucinfoArchivePaginator(),
            NewsSites.Alfahir => new  AlfahirArchivePaginator(),
            NewsSites.HuszonnegyHu => new  HuszonnegyArchivePaginator(),
            NewsSites.NegyNegyNegy => new  NegynegynegyArchivePaginator(),
            NewsSites.HVG => new  HvgArchivePaginator(),
            NewsSites.Telex => new  TelexArchivePaginator(),
            NewsSites.RTL => new  RtlArchivePaginator(),
            NewsSites.Index => new  IndexArchivePaginator(),
            NewsSites.Merce => new  MerceArchivePaginator(),
            _ => throw new ArgumentException($"Not supported news source: {source}"),
        };
    }
}
