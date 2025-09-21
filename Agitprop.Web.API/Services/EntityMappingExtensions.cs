using Agitprop.Core.Models;
using Agitprop.Web.Api.DTOs;

namespace Agitprop.Web.Api.Services;

public static class EntityMappingExtensions
{
    public static List<EntityDto> ToEntityDtos(this IEnumerable<Entity> entities)
    {
        return entities.Select(e => e.ToEntityDto()).ToList();
    }

    public static EntityDto ToEntityDto(this Entity entity)
    {
        return new EntityDto
        {
            Id = Guid.Parse(entity.Id),
            Name = entity.Name,
            MentionCount = entity.MentionCount,
            FirstMentioned = DateOnly.FromDateTime(entity.FirstMentioned),
            LastMentioned = DateOnly.FromDateTime(entity.LastMentioned),
            EntityType = entity.EntityType,
            Sentiment = entity.AverageSentiment
        };
    }

    public static EntityDetailsDto ToEntityDetailsDto(this Entity entity)
    {
        var entityDto = entity.ToEntityDto();
        
        return new EntityDetailsDto
        {
            Id = entityDto.Id,
            Name = entityDto.Name,
            MentionCount = entityDto.MentionCount,
            FirstMentioned = entityDto.FirstMentioned,
            LastMentioned = entityDto.LastMentioned,
            EntityType = entityDto.EntityType,
            Sentiment = entityDto.Sentiment,
            MentionTimeSeries = entity.MentionTimeSeries.Select(ts => new TimeSeriesDataPoint
            {
                Date = DateOnly.FromDateTime(ts.Date),
                Value = ts.Value
            }).ToList(),
            SentimentTimeSeries = entity.SentimentTimeSeries.Select(ts => new TimeSeriesDataPoint
            {
                Date = DateOnly.FromDateTime(ts.Date),
                Value = ts.Value
            }).ToList(),
            TopKeywords = entity.TopKeywords,
            TopCooccurringEntities = entity.CooccurringEntities.Select(e => e.Name).ToList()
        };
    }
}