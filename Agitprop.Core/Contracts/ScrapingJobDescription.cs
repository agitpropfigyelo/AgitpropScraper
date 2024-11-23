using Agitprop.Core.Enums;

namespace Agitprop.Core.Contracts
{
    public record ScrapingJobDescription
    {
        public Uri Url { get; init; }
        public PageContentType Type { get; init; }
    }
}