using Agitprop.Core.Contracts;
using Agitprop.Core.Enums;

namespace Agitprop.Core.Factories;

public static class JobDescriptionFactory
{


    public static ScrapingJobDescription GetAgitpropScrapingJob(NewsSites site)
    {
        return site switch
        {
            NewsSites.Origo => new ScrapingJobDescription { Url = new Uri($"https://www.origo.hu/hirarchivum/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.Ripost => new ScrapingJobDescription { Url = new Uri($"https://ripost.hu/{DateTime.Now:yyyyMM}_sitemap.xml"), Type = PageContentType.Archive },
            NewsSites.Mandiner => new ScrapingJobDescription { Url = new Uri($"https://mandiner.hu/{DateTime.Now:yyyyMM}_sitemap.xml"), Type = PageContentType.Archive },
            NewsSites.Metropol => new ScrapingJobDescription { Url = new Uri($"https://metropol.hu/{DateTime.Now:yyyyMM}_sitemap.xml"), Type = PageContentType.Archive },
            NewsSites.MagyarNemzet => new ScrapingJobDescription { Url = new Uri($"https://magyarnemzet.hu/{DateTime.Now:yyyyMM}_sitemap.xml"), Type = PageContentType.Archive },
            NewsSites.PestiSracok => new ScrapingJobDescription { Url = new Uri($"https://pestisracok.hu/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.MagyarJelen => new ScrapingJobDescription { Url = new Uri($"https://magyarjelen.hu/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.Kurucinfo => new ScrapingJobDescription { Url = new Uri("https://kuruc.info/to/1/20/"), Type = PageContentType.Archive },
            NewsSites.Alfahir => new ScrapingJobDescription { Url = new Uri("https://alfahir.hu/hirek/oldalak/1"), Type = PageContentType.Archive },
            NewsSites.Huszonnegy => new ScrapingJobDescription { Url = new Uri($"https://24.hu/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.NegyNegyNegy => new ScrapingJobDescription { Url = new Uri($"https://444.hu/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.HVG => new ScrapingJobDescription { Url = new Uri($"https://hvg.hu/frisshirek/{DateTime.Now.Year:D4}.{DateTime.Now.Month:D2}.{DateTime.Now.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.Telex => new ScrapingJobDescription { Url = new Uri($"https://telex.hu/sitemap/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}/news.xml"), Type = PageContentType.Archive },
            NewsSites.RTL => new ScrapingJobDescription { Url = new Uri("https://rtl.hu/legfrissebb?oldal=1"), Type = PageContentType.Archive },
            NewsSites.Index => new ScrapingJobDescription { Url = new Uri($"https://index.hu/sitemap/cikkek_{DateTime.Now:yyyyMM}.xml"), Type = PageContentType.Archive },
            NewsSites.Merce => new ScrapingJobDescription { Url = new Uri($"https://merce.hu/{DateTime.Now.Year:D4}/{DateTime.Now.Month:D2}/{DateTime.Now.Day:D2}"), Type = PageContentType.Archive },
            _ => throw new ArgumentException($"Not supported news source: {site}"),
        };
    }

    public static ScrapingJobDescription GetAgitpropScrapingJob(NewsSites site, DateOnly date)
    {
        return site switch
        {
            NewsSites.Origo => new ScrapingJobDescription { Url = new Uri($"https://www.origo.hu/hirarchivum/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.Ripost => new ScrapingJobDescription { Url = new Uri($"https://ripost.hu/{date:yyyyMM}_sitemap.xml"), Type = PageContentType.Archive },
            NewsSites.Mandiner => new ScrapingJobDescription { Url = new Uri($"https://mandiner.hu/{date:yyyyMM}_sitemap.xml"), Type = PageContentType.Archive },
            NewsSites.Metropol => new ScrapingJobDescription { Url = new Uri($"https://metropol.hu/{date:yyyyMM}_sitemap.xml"), Type = PageContentType.Archive },
            NewsSites.MagyarNemzet => new ScrapingJobDescription { Url = new Uri($"https://magyarnemzet.hu/{date:yyyyMM}_sitemap.xml"), Type = PageContentType.Archive },
            NewsSites.PestiSracok => new ScrapingJobDescription { Url = new Uri($"https://pestisracok.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.MagyarJelen => new ScrapingJobDescription { Url = new Uri($"https://magyarjelen.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.Kurucinfo => new ScrapingJobDescription { Url = new Uri("https://kuruc.info/to/1/20/"), Type = PageContentType.Archive },
            NewsSites.Alfahir => new ScrapingJobDescription { Url = new Uri("https://alfahir.hu/hirek/oldalak/1"), Type = PageContentType.Archive },
            NewsSites.Huszonnegy => new ScrapingJobDescription { Url = new Uri($"https://24.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.NegyNegyNegy => new ScrapingJobDescription { Url = new Uri($"https://444.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.HVG => new ScrapingJobDescription { Url = new Uri($"https://hvg.hu/frisshirek/{date.Year:D4}.{date.Month:D2}.{date.Day:D2}"), Type = PageContentType.Archive },
            NewsSites.Telex => new ScrapingJobDescription { Url = new Uri($"https://telex.hu/sitemap/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}/news.xml"), Type = PageContentType.Archive },
            NewsSites.RTL => new ScrapingJobDescription { Url = new Uri("https://rtl.hu/legfrissebb?oldal=1"), Type = PageContentType.Archive },
            NewsSites.Index => new ScrapingJobDescription { Url = new Uri($"https://index.hu/sitemap/cikkek_{date:yyyyMM}.xml"), Type = PageContentType.Archive },
            NewsSites.Merce => new ScrapingJobDescription { Url = new Uri($"https://merce.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}"), Type = PageContentType.Archive },
            _ => throw new ArgumentException($"Not supported news source: {site}"),
        };
    }
}