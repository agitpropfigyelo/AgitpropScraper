namespace Agitprop.Core.Interfaces;

public interface ILinkTracker
{
    Task AddVisitedLinkAsync(string visitedLink);
    Task<bool> WasLinkVisited(string link);
    Task<List<string>> GetVisitedLinksAsync();
    Task<List<string>> GetNotVisitedLinks(IEnumerable<string> links);
    Task<long> GetVisitedLinksCount();
}
