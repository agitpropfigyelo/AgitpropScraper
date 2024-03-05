namespace webscraper;

public class ArticleScraperFactory
{
private static Dictionary<string, IArticleScraperService> availableScrapers = new Dictionary<string, IArticleScraperService>(){
        {"origo", new OrigoArticleScraper()},
    };

    public static IArticleScraperService GetScraperForSite(string siteIn)
    {
        return availableScrapers[siteIn];
    }

    public static List<IArticleScraperService> GetScraperForSites(IEnumerable<string> sitesIn)
    {
        List<IArticleScraperService> result = new();
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
