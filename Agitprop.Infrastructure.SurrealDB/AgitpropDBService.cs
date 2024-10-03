using System;
using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.SurrealDB.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SurrealDb.Net;
using SurrealDb.Net.Models.Response;

namespace Agitprop.Infrastructure.SurrealDB
{
    public class AgitpropDBService : SurrealDBProvider, IAgitpropDataBaseService
    {
        private readonly static string selectEntityQuery = "select id from entity where Name=$en";

        private bool beenInitialized = false;
        private ILogger Logger;
        private IConfiguration configuration;

        public AgitpropDBService(ILogger logger) : base(logger)
        {
        }

        public async Task<int> CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities)
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
            return entities.All.Count;
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
    }
}
