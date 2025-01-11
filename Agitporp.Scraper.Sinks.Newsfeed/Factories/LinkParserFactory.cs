using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

namespace Agitporp.Scraper.Sinks.Newsfeed.Factories;

internal static class LinkParserFactory
{
    public static ILinkParser GetLinkParser(NewsSites siteIn)
    {
        return siteIn switch
        {
            NewsSites.Origo => new Scrapers.Origo.ArchiveLinkParser(),
            NewsSites.Ripost => new Scrapers.Ripost.ArchiveLinkParser(),
            NewsSites.Mandiner => new Scrapers.Mandiner.ArchiveLinkParser(),
            NewsSites.Metropol => new Scrapers.Metropol.ArchiveLinkParser(),
            NewsSites.MagyarNemzet => new Scrapers.Magyarnemzet.ArchiveLinkParser(),
            NewsSites.PestiSracok => new Scrapers.Pestisracok.ArchiveLinkParser(),
            NewsSites.MagyarJelen => new Scrapers.Magyarjelen.ArchiveLinkParser(),
            NewsSites.Kurucinfo => new Scrapers.Kurucinfo.ArchiveLinkParser(),
            NewsSites.Alfahir => new Scrapers.Alfahir.ArchiveLinkParser(),
            NewsSites.Huszonnegy => new Scrapers.Huszonnegy.ArchiveLinkParser(),
            NewsSites.NegyNegyNegy => new Scrapers.Negynegynegy.ArchiveLinkParser(),
            NewsSites.HVG => new Scrapers.Hvg.ArchiveLinkParser(),
            NewsSites.Telex => new Scrapers.Telex.ArchiveLinkParser(),
            NewsSites.RTL => new Scrapers.Rtl.ArchiveLinkParser(),
            NewsSites.Index => new Scrapers.Index.ArchiveLinkParser(),
            NewsSites.Merce => new Scrapers.Merce.ArchiveLinkParser(),
            _ => throw new ArgumentException($"Not supported news source: {siteIn}"),
        };
    }
}
