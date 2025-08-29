using Agitprop.Core;
using Agitprop.Core.Enums;

namespace Agitprop.Sinks.Newsfeed
{
    /// <summary>
    /// Represents a description of a newsfeed job, extending the base scraping job description.
    /// </summary>
    public class NewsfeedJobDescrpition : ScrapingJobDescription
    {
        /// <summary>
        /// Gets the type of content on the page.
        /// </summary>
        public PageContentType Type { get; init; }
    }
}
