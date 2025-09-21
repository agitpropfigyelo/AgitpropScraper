namespace Agitprop.Web.Api.DTOs;

public class EntityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MentionCount { get; set; }
    public DateOnly FirstMentioned { get; set; }
    public DateOnly LastMentioned { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public double Sentiment { get; set; }
}