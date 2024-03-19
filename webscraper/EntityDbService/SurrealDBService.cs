using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;

namespace webscraper;

public class SurrealDBService
{
    private readonly static string selectEntityQuery = "select * from entity where name=$en";
    private readonly static string relateQuery = "RELATE $src->mentions->$eid SET date=$d, url=$u;";
    private readonly static string articleExistsQuery = "select * from mentions where url=$en";

    private readonly static string endpoint = "http://localhost:8000";
    private readonly static string ns = "agitprop";
    private readonly static string dbName = "test";
    private readonly static string userName = "root";
    private readonly static string userPassword = "root";
    private SurrealDbClient CreateClient()
    {
        SurrealDbClient client = new(endpoint);
        client.Configure(ns, dbName, userName, userPassword);
        return client;
    }

    public async Task CreateMentionsForArticle(Article articleIn)
    {
        List<string> entityStrings = [];
        entityStrings.AddRange(articleIn.Entities!.MISC ??= []);
        entityStrings.AddRange(articleIn.Entities!.ORG ??= []);
        entityStrings.AddRange(articleIn.Entities!.PER ??= []);

        foreach (var item in entityStrings)
        {
            CreateMention($"source:{articleIn.Source}", GetEntityId(item), articleIn.Url, articleIn.Date);
        }

        // entityStrings.AsParallel().ForAll(async ent =>
        // {
        //     CreateMention($"source:{articleIn.Source}", await GetEntityId(ent), articleIn.Url, articleIn.Date);
        // });
    }

    public async Task<bool> IsArticleAlreadPresentAsync(Article articleIn)
    {
        SurrealDbClient client = CreateClient();
        Dictionary<string, object> parameters = new()
        {
            { "en", articleIn.Url },
        };
        SurrealDbResponse result = await client.RawQuery(relateQuery, parameters);

        Mentions? mentions = result.FirstOk.GetValues<Mentions>().FirstOrDefault();
        return mentions is not null;
    }

    private void CreateMention(string sourceIn, string entityIn, Uri url, DateTime date)
    {
        SurrealDbClient client = CreateClient();
        Dictionary<string, object> parameters = new()
        {
            { "src", sourceIn },
            { "eid", entityIn },
            { "d", date },
            { "u", url }
        };
        client.RawQuery(relateQuery, parameters);
    }

    public string CreateEntity(string entityName)
    {
        SurrealDbClient client = CreateClient();
        Entity result = client.Create<Entity>("entity", new Entity { Name = entityName }).Result;
        return result.Id.ToString();
    }
    public string GetEntityId(string entityName)
    {
        SurrealDbClient client = CreateClient();
        Dictionary<string, object> parameters = new()
        {
            { "en", entityName },
        };
        SurrealDbResponse result = client.RawQuery(selectEntityQuery, parameters).Result;
        Entity? ent = result.FirstOk.GetValues<Entity>().FirstOrDefault();
        return ent?.Id.ToString() ?? CreateEntity(entityName);
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
        public Source In { get; set; }
        public Entity Out { get; set; }
        public string Url { get; set; }
        public string Date { get; set; }
    }
}
