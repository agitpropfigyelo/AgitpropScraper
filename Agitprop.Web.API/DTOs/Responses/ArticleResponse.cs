namespace Agitprop.Web.Api.DTOs.Responses;

public class ArticleResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateOnly PublishedAt { get; set; }
    public double Sentiment { get; set; }
    public List<string> MentionedEntities { get; set; } = new();
}