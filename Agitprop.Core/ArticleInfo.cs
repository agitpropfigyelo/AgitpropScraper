namespace NewsArticleScraper.Core;

public class ArticleInfo
{
    public string Url { get; set; }
    public string Content { get; set; }
    public DateTime PublicationDate { get; set; }
    public List<string>? entities;

    public override string ToString()
    {
        return $"{Url}\n{PublicationDate:d}\n{Content[..20]}";
    }
}
