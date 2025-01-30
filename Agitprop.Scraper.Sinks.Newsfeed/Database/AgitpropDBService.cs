using Agitprop.Scraper.Sinks.Newsfeed.Database.Models;
using Agitprop.Scraper.Sinks.Newsfeed.Interfaces;

using Agitprop.Core;

using Microsoft.Extensions.Logging;

using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;

namespace Agitprop.Scraper.Sinks.Newsfeed.Database;

public class NewsfeedDB : INewsfeedDB
{
    private readonly static string selectEntityQuery = "select id from entity where Name=$en";
    private ILogger<NewsfeedDB> Logger;
    private ISurrealDbClient Client;

    public NewsfeedDB(ILogger<NewsfeedDB> logger, ISurrealDbClient client)
    {
        this.Logger = logger;
        this.Client = client;
    }

    //TODO: rework insertions, might gain performance
    public async Task<int> CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities)
    {
        try
        {
            var src = RecordId.From("source", $"{article.SourceSite}");
            var mention = new Mentions { Date = article.PublishDate, Url = url };
            var entIds = entities.All.Select(e => GetOrAddEntityAsync(e).Result.Id);
            var kdi = await Client.Relate<Mentions, Mentions>("mentions", src, entIds, mention);
            Logger.LogInformation($"{url} added mentions ({entIds.Count()})");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to create mentions: {url} EX: {ex.Message}");
        }
        return entities.All.Count;
    }

    public async Task<bool> IsUrlAlreadyExists(string url)
    {
        var result = await Client.RawQuery("(SELECT Link FROM visitedLinks).Link.any(|$var| $var.is_string());", new Dictionary<string, object?> { { "var", url } });
        return result.GetValue<bool>(0);
    }

    private async Task<Entity> CreateEntityAsync(string entityName)
    {
        Entity result = await Client.Create("entity", new Entity { Name = entityName });
        Logger.LogInformation($"Added entity {result.Name}");
        return result;
    }

    private async Task<Entity> GetOrAddEntityAsync(string entityName)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "en", entityName },
            };
        SurrealDbResponse result = await Client.RawQuery(selectEntityQuery, parameters);
        Entity ent = result.FirstOk.GetValues<Entity>().FirstOrDefault() ?? await CreateEntityAsync(entityName);
        Logger.LogInformation($"Get entity {entityName} : {ent.Id.DeserializeId<string>()} - {ent.Name}");
        return ent;
    }
}
