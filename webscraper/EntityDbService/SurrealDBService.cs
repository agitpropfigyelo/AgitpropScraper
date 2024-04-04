using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;

namespace webscraper
{
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
            SurrealDbClient client = new SurrealDbClient(endpoint);
            client.Configure(ns, dbName, userName, userPassword);
            return client;
        }

        public async Task CreateMentionsForArticle(Article articleIn, IProgress<int>? progress = null, CancellationToken? cancellationToken = null)
        {
            if (articleIn.Entities is null)
            {
                progress?.Report(1);
                throw new InvalidOperationException($"No entities for article: {articleIn.Url}");
            }
            Task<Task>[] tasks = articleIn.Entities.Select(async item => CreateMention($"source:{articleIn.Source}", await GetEntityIdAsync(item), articleIn.Url, articleIn.Date)).ToArray();
            await Task.WhenAll(tasks);
            progress?.Report(1);
        }

        public async Task<bool> IsArticleAlreadPresentAsync(Article articleIn)
        {
            SurrealDbClient client = CreateClient();
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "en", articleIn.Url },
            };
            SurrealDbResponse result = await client.RawQuery(relateQuery, parameters);

            Mentions? mentions = result.FirstOk.GetValues<Mentions>().FirstOrDefault();
            return mentions != null;
        }

        private async Task CreateMention(string sourceIn, string entityIn, Uri url, DateTime date)
        {
            SurrealDbClient client = CreateClient();
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "src", sourceIn },
                { "eid", entityIn },
                { "d", date },
                { "u", url }
            };
            await client.RawQuery(relateQuery, parameters);
        }

        public async Task<string> CreateEntityAsync(string entityName)
        {
            SurrealDbClient client = CreateClient();
            Entity result = await client.Create<Entity>("entity", new Entity { Name = entityName });
            return result.Id.ToString();
        }

        public async Task<string> GetEntityIdAsync(string entityName)
        {
            SurrealDbClient client = CreateClient();
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "en", entityName },
            };
            SurrealDbResponse result = await client.RawQuery(selectEntityQuery, parameters);
            Entity? ent = result.FirstOk.GetValues<Entity>().FirstOrDefault();
            return ent?.Id.ToString() ?? await CreateEntityAsync(entityName);
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
}
