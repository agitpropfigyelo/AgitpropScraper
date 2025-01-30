using Agitprop.Core;

namespace Agitprop.Scraper.Sinks.Newsfeed_Test;
public partial class ContentParserTests
{
    public class TestCase
    {
        public string HtmlPath { get; set; }

        public ContentParserResult ExpectedContent { get; set; }
    }
}
