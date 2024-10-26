using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Factories;
using Agitprop.Scrapers.Factories;

namespace Agitprop.Worker;

public class StartJobFactory
{
    ScrapingJobFactory scrapingJobFactory = new();

    public ScrapingJob GetAgitpropScrapingJob(NewsSites site)
    {
        return site switch
        {
            NewsSites.Origo => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://www.origo.hu/hirarchivum/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"),
            NewsSites.Ripost => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://ripost.hu/{DateTime.Now:yyyyMM}_sitemap.xml"),
            NewsSites.Mandiner => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://mandiner.hu/{DateTime.Now:yyyyMM}_sitemap.xml"),
            NewsSites.Metropol => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://metropol.hu/{DateTime.Now:yyyyMM}_sitemap.xml"),
            NewsSites.MagyarNemzet => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://magyarnemzet.hu/{DateTime.Now:yyyyMM}_sitemap.xml"),
            NewsSites.PestiSracok => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://pestisracok.hu/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"),
            NewsSites.MagyarJelen => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://magyarjelen.hu/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"),
            NewsSites.Kuruczinfo => scrapingJobFactory.GetArchiveScrapingJob(site, "https://kuruc.info/to/1/20/"),
            NewsSites.Alfahir => scrapingJobFactory.GetArchiveScrapingJob(site, "https://alfahir.hu/hirek/oldalak/1"),
            NewsSites.Huszonnegy => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://24.hu/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"),
            NewsSites.NegyNegyNegy => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://444.hu/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"),
            NewsSites.HVG => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://hvg.hu/frisshirek/{DateTime.Now.Year:D4}.{DateTime.Now.Month:D2}.{DateTime.Now.Day:D2}"),
            NewsSites.Telex => scrapingJobFactory.GetArchiveScrapingJob(site, "https://telex.hu/legfrissebb?oldal=1"),
            NewsSites.RTL => scrapingJobFactory.GetArchiveScrapingJob(site, "https://rtl.hu/legfrissebb?oldal=1"),
            NewsSites.Index => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://index.hu/sitemap/cikkek_{DateTime.Now:yyyyMM}.xml"),
            NewsSites.Merce => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://merce.hu/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"),
            _ => throw new ArgumentException($"Not supported news source: {site}"),
        };
    }

    public ScrapingJob GetAgitpropScrapingJob(NewsSites site,DateOnly date)
    {
        return site switch
        {
            NewsSites.Origo => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://www.origo.hu/hirarchivum/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"),
            NewsSites.Ripost => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://ripost.hu/{date:yyyyMM}_sitemap.xml"),
            NewsSites.Mandiner => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://mandiner.hu/{date:yyyyMM}_sitemap.xml"),
            NewsSites.Metropol => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://metropol.hu/{date:yyyyMM}_sitemap.xml"),
            NewsSites.MagyarNemzet => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://magyarnemzet.hu/{date:yyyyMM}_sitemap.xml"),
            NewsSites.PestiSracok => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://pestisracok.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"),
            NewsSites.MagyarJelen => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://magyarjelen.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"),
            NewsSites.Kuruczinfo => scrapingJobFactory.GetArchiveScrapingJob(site, "https://kuruc.info/to/1/20/"),
            NewsSites.Alfahir => scrapingJobFactory.GetArchiveScrapingJob(site, "https://alfahir.hu/hirek/oldalak/1"),
            NewsSites.Huszonnegy => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://24.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"),
            NewsSites.NegyNegyNegy => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://444.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"),
            NewsSites.HVG => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://hvg.hu/frisshirek/{date.Year:D4}.{date.Month:D2}.{date.Day:D2}"),
            NewsSites.Telex => scrapingJobFactory.GetArchiveScrapingJob(site, "https://telex.hu/legfrissebb?oldal=1"),
            NewsSites.RTL => scrapingJobFactory.GetArchiveScrapingJob(site, "https://rtl.hu/legfrissebb?oldal=1"),
            NewsSites.Index => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://index.hu/sitemap/cikkek_{date:yyyyMM}.xml"),
            NewsSites.Merce => scrapingJobFactory.GetArchiveScrapingJob(site, $"https://merce.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"),
            _ => throw new ArgumentException($"Not supported news source: {site}"),
        };
    }
}
