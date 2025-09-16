using Agitprop.Core;

using Microsoft.Extensions.Logging;

using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;
using Agitprop.Infrastructure.SurrealDB.Models;
using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure.SurrealDB;

/// <summary>
/// Represents the database operations for the Newsfeed system.
/// </summary>
public class NewsfeedDB : INewsfeedDB
{
    private readonly static string selectEntityQuery = "select id from entity where Name=$en";
    private ILogger<NewsfeedDB> Logger;
    private ISurrealDbClient Client;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsfeedDB"/> class.
    /// </summary>
    /// <param name="logger">The logger for logging information and errors.</param>
    /// <param name="client">The SurrealDB client for database operations.</param>
    public NewsfeedDB(ILogger<NewsfeedDB> logger, ISurrealDbClient client)
    {
        Logger = logger;
        Client = client;
    }

    /// <summary>
    /// Creates mentions for a given article URL, parser result, and named entities.
    /// </summary>
    /// <param name="url">The URL of the article.</param>
    /// <param name="parserResult">The result of parsing the article content.</param>
    /// <param name="entities">The named entities extracted from the article.</param>
    /// <returns>The number of mentions created.</returns>
    public async Task<int> CreateMentionsAsync(string url, ContentParserResult parserResult, NamedEntityCollection entities)
    {
        var src = RecordId.From("source", $"{parserResult.SourceSite}");

        var article = await Client.Create("articles", new ArticleRecord { Url = url, PublishedTime = parserResult.PublishDate.DateTime });
        Logger?.LogInformation("Added article {art}", article);
        //add publish
        var published = await Client.Relate<PublishedRelation>("published", src, article.Id);
        Logger?.LogInformation("Added published relation for {url} with id {id}", url, published.Id);
        //add mentions
        var entIds = entities.All.Select(e => GetOrAddEntityAsync(e).Result.Id);
        var mentions = await Client.Relate<MentionsRelation>("mentions", article.Id, entIds);
        Logger?.LogInformation("Added {count} mentions for {url}", mentions.Count(), url);

        return mentions.Count();
    }

    /// <summary>
    /// Checks if a URL already exists in the database.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns>True if the URL exists; otherwise, false.</returns>
    public async Task<bool> IsUrlAlreadyExists(string url)
    {
        var result = await Client.RawQuery("RETURN count(SELECT Url FROM articles WHERE Url=$var)>0;", new Dictionary<string, object?> { { "var", url } });
        return result.GetValue<bool>(0);
    }

    /// <summary>
    /// Creates a new entity in the database.
    /// </summary>
    /// <param name="entityName">The name of the entity to create.</param>
    /// <returns>The created entity.</returns>
    private async Task<EntityRecord> CreateEntityAsync(string entityName)
    {
        EntityRecord result = await Client.Create("entity", new EntityRecord { Name = entityName });
        Logger.LogInformation("Added entity {name}", result.Name);
        return result;
    }

    /// <summary>
    /// Retrieves an entity by name or creates it if it does not exist.
    /// </summary>
    /// <param name="entityName">The name of the entity to retrieve or create.</param>
    /// <returns>The retrieved or created entity.</returns>
    private async Task<EntityRecord> GetOrAddEntityAsync(string entityName)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "en", entityName },
            };
        SurrealDbResponse result = await Client.RawQuery(selectEntityQuery, parameters);
        EntityRecord ent = result.FirstOk.GetValues<EntityRecord>().FirstOrDefault() ?? await CreateEntityAsync(entityName);
        Logger.LogInformation("Get entity {entityName} : {id} - {name}", entityName, ent.Id.DeserializeId<string>(), ent.Name);
        return ent;
    }
}
