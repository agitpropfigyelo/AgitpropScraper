using Agitprop.Infrastructure.Enums;
using Agitprop.Infrastructure.Interfaces;

namespace Agitprop.Infrastructure;

public record ScrapingJob
{
    public string Url { get; init; }
    public PageCategory PageCategory { get; init; }
    public PageType PageType { get; init; }
    public IEnumerable<PageAction> Actions { get; init; }
    public IEnumerable<IContentParser> ContentParsers { get; init; }
    public IEnumerable<ILinkParser> LinkParsers { get; init; }
    public IPaginator? Pagination { get; init; }

}
