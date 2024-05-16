namespace NewsArticleScraper.Core;

public interface IScraperFactory
{
    INewsSiteScraper GetScraperForSite(NewsSites siteIn);
}
