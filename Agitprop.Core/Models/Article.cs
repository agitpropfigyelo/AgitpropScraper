namespace Agitprop.Core.Models
{
    public class Article
    {
        /// <summary>
        /// Gets the URL of the article.
        /// </summary>
        public string Url { init; get; }

        /// <summary>
        /// Gets the publication time of the article.
        /// </summary>
        public DateTime PublishedTime { init; get; }
    }
}
