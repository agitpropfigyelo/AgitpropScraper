using Agitprop.Core;
using Agitprop.Core.Enums;

namespace Agitprop.Scraper.Sinks.Newsfeed
{
    public class NewsfeedJobDescrpition : ScrapingJobDescription
    {
        public PageContentType Type { get; init; }
    }
}
