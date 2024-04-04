using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class NewsSiteScraperFactory : IScraperFactory
{
    public NewsSiteScraperFactory()
    {

    }
    public INewsSiteScraper GetScraperForSite(NewsSites siteIn)
    {
        return siteIn switch
        {
            NewsSites.Origo => new OrigoScraper(),
            NewsSites.Ripost => new RipostScraper(),
            NewsSites.Mandiner => new MandinerScraper(),
            NewsSites.Metropol => new MetropolScraper(),
            NewsSites.MagyarNemzet => new MagyarNemzetScraper(),
            NewsSites.PestiSracok => new PestriSracokScraper(),
            NewsSites.MagyarJelen => new MagyarJelenScraper(),
            NewsSites.Kuruczinfo => new KuruczinfoScraper(),
            NewsSites.Alfahir => new AlfahirScraper(),
            NewsSites.Huszonnegy => new HuszonnegyScraper(),
            NewsSites.NegyNegyNegy => new NegynegynegyScraper(),
            NewsSites.HVG => new HvgScraper(),
            NewsSites.Telex => new TelexScraper(),
            NewsSites.RTL => new RtlScraper(),
            NewsSites.Index => new IndexScraper(),
            NewsSites.Merce => new MerceScraper(),
            _ => throw new ArgumentException($"Not supported news source: {siteIn}"),
        };
    }
}
