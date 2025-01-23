namespace Agitprop.Scraper.Sinks.Newsfeed_Test;
using Agitprop.Core;

public partial class ContentParserTests
{
    public class TestCase
    {
        public string HtmlPath { get; set; }
        public ContentParserResult ExpectedContent { get; set; }
    }
}
