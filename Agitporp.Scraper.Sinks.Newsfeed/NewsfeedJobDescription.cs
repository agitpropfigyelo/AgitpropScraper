using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed;

public record NewsfeedJobDescrpition
{
    public string Url { get; init; }
    public PageContentType Type { get; init; }
}