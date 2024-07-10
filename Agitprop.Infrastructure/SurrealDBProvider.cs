using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Markup;
using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Auth;
using SurrealDb.Net.Models.Response;

namespace Agitprop.Infrastructure;

public class SurrealDBProvider : IAgitpropDataBaseService, ILinkTracker
{
    private const string visitedLinksTable = "visitedLinks";
    private readonly static string selectEntityQuery = "select id from entity where Name=$en";
    private readonly static string relateQuery = "RELATE $src->mentions->$eid SET Date=$d, Url=$u;";
    private readonly static string articleExistsQuery = "select * from mentions where Url=$en";

    private readonly static string endpoint = "ws://127.0.0.1:8000/rpc";
    private readonly static string ns = "agitprop";
    private readonly static string dbName = "test";
    private readonly static string userName = "root";
    private readonly static string userPassword = "root";
    private ILogger Logger;

    public Task Initialization => Initialize();
    private bool beenInitialized = false;

    public SurrealDBProvider(ILogger logger)
    {
        Logger = logger;
    }

    public async Task Initialize()
    {
        if (beenInitialized) return;
        var client = await CreateClientAsync();
        await client.RawQuery(File.ReadAllText("db_init.surql"));

    }

    public async Task CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities)
    {
        try
        {
            var client = await CreateClientAsync();
            List<string> entids = [];
            await client.Set("$date", article.PublishDate);
            await client.Set("$url", url);
            string src = $"source:{article.SourceSite}";
            await client.Set("$src", $"source:{article.SourceSite}");
            foreach (var item in entities.All)
            {
                //entids.Add((await GetOrAddEntityAsync(client, item)).Id.ToString());
                var entId = (await GetOrAddEntityAsync(client, item)).Id;
                await client.Set("$ent", entId);
                var str = $"RELATE $src->mentions->$ent SET Date=$date, Url=$url;";
                var idk = await client.Query($"RELATE {src}->mentions->{entId} SET Date={article.PublishDate}, Url={url};");
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to create mentions: {url} EX: {ex.Message}");
        }
    }

    private async Task<SurrealDbClient> CreateClientAsync()
    {
        var db = new SurrealDbClient(endpoint);

        await db.SignIn(new RootAuth { Username = userName, Password = userPassword });
        await db.Use(ns, dbName);
        return db;
    }

    private async Task<Entity> CreateEntityAsync(SurrealDbClient client, string entityName)
    {
        Entity result = await client.Create<Entity>("entity", new Entity { Name = entityName });
        return result;
    }

    private async Task<Entity> GetOrAddEntityAsync(SurrealDbClient client, string entityName)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "en", entityName },
            };
        SurrealDbResponse result = await client.RawQuery(selectEntityQuery, parameters);
        Entity? ent = result.FirstOk.GetValues<Entity>().FirstOrDefault();
        return ent ?? await CreateEntityAsync(client, entityName);
    }

    public async Task AddVisitedLinkAsync(string visitedLink)
    {
        try
        {
            var client = await CreateClientAsync();
            await client.RawQuery($"CREATE visitedLinks SET Link='{visitedLink}';");

        }
        catch (System.Exception ex)
        {

            throw;
        }
    }

    public async Task<List<string>> GetVisitedLinksAsync()
    {
        var client = await CreateClientAsync();
        var result = await client.Select<VisitedLink>(visitedLinksTable);
        return result.Select(link => link.Link).ToList();
    }

    public async Task<List<string>> GetNotVisitedLinks(IEnumerable<string> links)
    {
        try
        {
            var client = await CreateClientAsync();

            FormattableString str = $"return array::complement([{string.Join(',', links.Select(x => $"'Name':'{x}'"))}],(SELECT Link FROM visitedLinks));";
            var idk = await client.Query(str);
            return [];
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to qurey EX: {ex.Message}");
        }
        return [];
    }

    public async Task<long> GetVisitedLinksCount()
    {
        var str = "return count(Select * FROM visitedLinks);";
        var client = await CreateClientAsync();
        var result = await client.RawQuery(str);
        return result.FirstOk.GetValue<long>();
    }

    private class VisitedLink : Record
    {
        public string Link;
    }
    private class Entity : Record
    {
        public string Name { get; set; }
    }

    private class Source : Record
    {
        public string Src { get; set; }
    }

    private class Mentions : Record
    {
        public Source @in { get; set; }
        public Entity @out { get; set; }
        public string Url { get; set; }
        public string Date { get; set; }
    }
}
