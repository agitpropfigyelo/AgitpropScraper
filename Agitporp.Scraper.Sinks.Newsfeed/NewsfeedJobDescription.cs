using Agitprop.Core;
using Agitprop.Core.Enums;

namespace Agitporp.Scraper.Sinks.Newsfeed
{
    public class NewsfeedJobDescrpition : ScrapingJobDescription
    {
        public PageContentType Type { get; init; }
    }
}
