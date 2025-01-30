using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ArchiveLinkParsers;

namespace Agitprop.Scraper.Sinks.Newsfeed.Factories;

internal static class LinkParserFactory
{
    public static ILinkParser GetLinkParser(NewsSites siteIn)
    {
        return siteIn switch
        {
            NewsSites.Origo => new OrigoArchiveLinkParser(),
            NewsSites.Ripost => new  RipostArchiveLinkParser(),
            NewsSites.Mandiner => new  MandinerArchiveLinkParser(),
            NewsSites.Metropol => new  MetropolArchiveLinkParser(),
            NewsSites.MagyarNemzet => new  MagyarNemzetArchiveLinkParser(),
            NewsSites.PestiSracok => new  PestiSracokArchiveLinkParser(),
            NewsSites.MagyarJelen => new  MagyarJelenArchiveLinkParser(),
            NewsSites.Kurucinfo => new  KurucinfoArchiveLinkParser(),
            NewsSites.Alfahir => new  AlfahirArchiveLinkParser(),
            NewsSites.Huszonnegy => new  HuszonnegyArchiveLinkParser(),
            NewsSites.NegyNegyNegy => new  NegynegynegyArchiveLinkParser(),
            NewsSites.HVG => new  HvgArchiveLinkParser(),
            NewsSites.Telex => new  TelexArchiveLinkParser(),
            NewsSites.RTL => new  RtlArchiveLinkParser(),
            NewsSites.Index => new  IndexArchiveLinkParser(),
            NewsSites.Merce => new  MerceArchiveLinkParser(),
            _ => throw new ArgumentException($"Not supported news source: {siteIn}"),
        };
    }
}
