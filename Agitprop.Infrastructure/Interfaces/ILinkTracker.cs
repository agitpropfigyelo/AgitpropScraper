namespace Agitprop.Infrastructure.Interfaces;

public interface ILinkTracker
{
    Task AddVisitedLinkAsync(string visitedLink);
    Task<List<string>> GetVisitedLinksAsync();
    Task<List<string>> GetNotVisitedLinks(IEnumerable<string> links);
    Task<long> GetVisitedLinksCount();

    Task Initialization { get; }
}
