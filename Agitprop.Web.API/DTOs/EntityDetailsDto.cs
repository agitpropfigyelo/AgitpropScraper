namespace Agitprop.Web.Api.DTOs;

public class EntityDetailsDto : EntityDto
{
    public List<TimeSeriesDataPoint> MentionTimeSeries { get; set; } = new();
    public List<TimeSeriesDataPoint> SentimentTimeSeries { get; set; } = new();
    public List<string> TopKeywords { get; set; } = new();
    public List<string> TopCooccurringEntities { get; set; } = new();
}

public class TimeSeriesDataPoint
{
    public DateOnly Date { get; set; }
    public double Value { get; set; }
}