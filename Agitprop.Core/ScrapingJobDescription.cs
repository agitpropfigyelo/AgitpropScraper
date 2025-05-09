namespace Agitprop.Core
{
    /// <summary>
    /// Represents a description of a web scraping job.
    /// </summary>
    public class ScrapingJobDescription
    {
        /// <summary>
        /// Gets or sets the URL to be scraped.
        /// </summary>
        public required string Url { get; set; }
    }
}
