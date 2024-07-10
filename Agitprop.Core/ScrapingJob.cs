using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

namespace Agitprop.Core;

public record ScrapingJob
{
    public string Url { get; init; }
    public PageCategory PageCategory { get; init; }
    public PageType PageType { get; init; }
    public List<PageAction> Actions { get; init; }
    public IEnumerable<IContentParser> ContentParsers { get; init; }
    public IEnumerable<ILinkParser> LinkParsers { get; init; }
    public IPaginator? Pagination { get; init; }

}
