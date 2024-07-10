using Agitprop.Core.Enums;
using Agitprop.Core.Factories;
using Agitprop.Core.Interfaces;

namespace Agitprop.Scrapers.Factories;

public class LinkParserFactory : ILinkParserFactory
{
    public ILinkParser GetLinkParser(NewsSites siteIn)
    {
        return siteIn switch
        {
            NewsSites.Origo => new Origo.ArchiveLinkParser(),
            NewsSites.Ripost => new Ripost.ArchiveLinkParser(),
            NewsSites.Mandiner => new Mandiner.ArchiveLinkParser(),
            NewsSites.Metropol => new Metropol.ArchiveLinkParser(),
            NewsSites.MagyarNemzet => new Magyarnemzet.ArchiveLinkParser(),
            NewsSites.PestiSracok => new Pestisracok.ArchiveLinkParser(),
            NewsSites.MagyarJelen => new Magyarjelen.ArchiveLinkParser(),
            NewsSites.Kuruczinfo => new Kuruczinfo.ArchiveLinkParser(),
            NewsSites.Alfahir => new Alfahir.ArchiveLinkParser(),
            NewsSites.Huszonnegy => new Huszonnegy.ArchiveLinkParser(),
            NewsSites.NegyNegyNegy => new Negynegynegy.ArchiveLinkParser(),
            NewsSites.HVG => new Hvg.ArchiveLinkParser(),
            NewsSites.Telex => new Telex.ArchiveLinkParser(),
            NewsSites.RTL => new Rtl.ArchiveLinkParser(),
            NewsSites.Index => new Index.ArchiveLinkParser(),
            NewsSites.Merce => new Merce.ArchiveLinkParser(),
            _ => throw new ArgumentException($"Not supported news source: {siteIn}"),
        };
    }
}
