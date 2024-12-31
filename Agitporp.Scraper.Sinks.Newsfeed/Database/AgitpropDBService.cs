using Agitporp.Scraper.Sinks.Newsfeed.Database.Models;
using Agitporp.Scraper.Sinks.Newsfeed.Interfaces;
using Agitprop.Core;
using Microsoft.Extensions.Logging;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;

namespace Agitporp.Scraper.Sinks.Newsfeed.Database;

public class NewsfeedDB : INewsfeedDB
{
    private readonly static string selectEntityQuery = "select id from entity where Name=$en";
    private ILogger<NewsfeedDB> logger;
    private ISurrealDbClient client;

    public NewsfeedDB(ILogger<NewsfeedDB> logger, ISurrealDbClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task<int> CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities)
    {
        try
        {
            var src = RecordId.From("source", $"{article.SourceSite}");
            var mention = new Mentions { Date = article.PublishDate, Url = url };
            var entIds = entities.All.Select(e => GetOrAddEntityAsync(e).Result.Id);
            var kdi = await client.Relate<Mentions, Mentions>("mentions", src, entIds, mention);
            logger.LogInformation($"{url} added mentions ({entIds.Count()})");
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to create mentions: {url} EX: {ex.Message}");
        }
        return entities.All.Count;
    }

    private async Task<Entity> CreateEntityAsync(string entityName)
    {
        Entity result = await client.Create("entity", new Entity { Name = entityName });
        logger.LogInformation($"Added entity {result.Name}");
        return result;
    }

    private async Task<Entity> GetOrAddEntityAsync(string entityName)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "en", entityName },
            };
        SurrealDbResponse result = await client.RawQuery(selectEntityQuery, parameters);
        Entity ent = result.FirstOk.GetValues<Entity>().FirstOrDefault() ?? await CreateEntityAsync(entityName);
        logger.LogInformation($"Get entity {entityName} : {ent.Id.DeserializeId<string>()} - {ent.Name}");
        return ent;
    }
}
