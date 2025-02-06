using System;

using SurrealDb.Net.Models;

namespace Agitprop.Scraper.Sinks.Newsfeed.Database.Models;

public class Article: Record
{
    public string Url{init; get;}
    public DateTime PublishedTime{init; get;}
}
