namespace Agitprop.Web.Api.DTOs;

public class NetworkItemDto
{
    public Guid EntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int CooccurrenceCount { get; set; }
    public double AverageSentiment { get; set; }
}