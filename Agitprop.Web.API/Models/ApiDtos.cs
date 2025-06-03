// DTO for entity search result
public class EntityDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}

// DTO for mentions result
public class MentionDto
{
    public string ArticleUrl { get; set; }
    public DateTime ArticlePublishedTime { get; set; }
    public DateTimeOffset MentionDate { get; set; }
}

// DTO for trending result (per entity, per day)
public class TrendingEntityDayDto
{
    public string EntityName { get; set; }
    public string Date { get; set; } // yyyy-MM-dd
    public int MentionCount { get; set; }
}

// DTO for trending API response (per day, top N entities)
public class TrendingDayDto
{
    public string Date { get; set; } // yyyy-MM-dd
    public List<TrendingEntityDayDto> TopEntities { get; set; }
}
