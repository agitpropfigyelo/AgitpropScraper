using System.ComponentModel;

namespace webscraper;

public static class ArchiveScraperFactory
{
    private static Dictionary<string, IArchiveScraperService> availableScrapers = new Dictionary<string, IArchiveScraperService>(){
        {"origo", new OrigoArchiveScraper()},
    };

    public static IArchiveScraperService GetScraperForSite(string siteIn)
    {
        return availableScrapers[siteIn];
    }

    public static List<IArchiveScraperService> GetScraperForSites(IEnumerable<string> sitesIn)
    {
        List<IArchiveScraperService> result = new();
        foreach (string site in sitesIn)
        {
            try
            {
                result.Add(GetScraperForSite(site));
            }
            catch (System.Exception)
            {
                System.Console.WriteLine($"No archive scraper available for {site}");
            }
        }
        return result;
    }
}
