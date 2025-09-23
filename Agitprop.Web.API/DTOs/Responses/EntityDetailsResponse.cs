namespace Agitprop.Web.Api.DTOs.Responses;

public class TimeSeriesDataPoint
{
    public DateOnly Date { get; set; }
    public double Value { get; set; }
}

public class EntityDetailsResponse : EntityResponse
{
    public List<TimeSeriesDataPoint> MentionTimeSeries { get; set; } = new();
    public List<TimeSeriesDataPoint> SentimentTimeSeries { get; set; } = new();
    public List<string> TopKeywords { get; set; } = new();
    public List<string> TopCooccurringEntities { get; set; } = new();
}