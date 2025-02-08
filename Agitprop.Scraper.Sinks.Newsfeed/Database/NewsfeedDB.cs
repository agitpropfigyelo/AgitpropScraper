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

    public async Task<int> CreateMentionsAsync(string url, ContentParserResult parserResult, NamedEntityCollection entities)
    {
        var src = RecordId.From("source", $"{parserResult.SourceSite}");

        var article = await Client.Create("articles", new Article { Url = url, PublishedTime = parserResult.PublishDate.DateTime });
        Logger?.LogInformation("Added article {art}", article);
        //add publish
        var published = await Client.Relate<Published>("published", src, article.Id);
        Logger?.LogInformation("Added published relation for {url} with id {id}", url,published.Id);
        //add mentions
        var entIds = entities.All.Select(e => GetOrAddEntityAsync(e).Result.Id);
        var mentions = await Client.Relate<Mentions>("mentions", article.Id, entIds);
        Logger?.LogInformation("Added {count} mentions for {url}", mentions.Count(), url);

        return mentions.Count();
    }

    public async Task<bool> IsUrlAlreadyExists(string url)
    {
        var result = await Client.RawQuery("(SELECT Url FROM articles).Url.any(|$var| $var.is_string());", new Dictionary<string, object?> { { "var", url } });
        return result.GetValue<bool>(0);
    }

    private async Task<Entity> CreateEntityAsync(string entityName)
    {
        Entity result = await Client.Create("entity", new Entity { Name = entityName });
        Logger.LogInformation("Added entity {name}", result.Name);
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
        Logger.LogInformation("Get entity {entityName} : {id} - {name}", entityName, ent.Id.DeserializeId<string>(), ent.Name);
        return ent;
    }
}
