using System;
using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.SurrealDB.Models;
using Microsoft.Extensions.Logging;
using SurrealDb.Net;
using SurrealDb.Net.Models.Response;

namespace Agitprop.Infrastructure.SurrealDB
{
    public class AgitpropDBService : IAgitpropDataBaseService
    {
        private readonly static string selectEntityQuery = "select id from entity where Name=$en";
        private ILogger<AgitpropDBService> logger;
        private ISurrealDbClient client;


        public async Task<int> CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities)
        {
            try
            {
                List<string> entids = [];
                await client.Set("$date", article.PublishDate);
                await client.Set("$url", url);
                string src = $"source:{article.SourceSite}";
                await client.Set("$src", $"source:{article.SourceSite}");
                foreach (var item in entities.All)
                {
                    //entids.Add((await GetOrAddEntityAsync(client, item)).Id.ToString());
                    var entId = (await GetOrAddEntityAsync(item)).Id;
                    await client.Set("$ent", entId);
                    var str = $"RELATE $src->mentions->$ent SET Date=$date, Url=$url;";
                    var idk = await client.Query($"RELATE {src}->mentions->{entId} SET Date={article.PublishDate}, Url={url};");
                }
            }
            catch (System.Exception ex)
            {
                logger.LogError($"Failed to create mentions: {url} EX: {ex.Message}");
            }
            return entities.All.Count;
        }

        private async Task<Entity> CreateEntityAsync(string entityName)
        {
            Entity result = await client.Create<Entity>("entity", new Entity { Name = entityName });
            return result;
        }

        private async Task<Entity> GetOrAddEntityAsync(string entityName)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "en", entityName },
                };
            SurrealDbResponse result = await client.RawQuery(selectEntityQuery, parameters);
            Entity? ent = result.FirstOk.GetValues<Entity>().FirstOrDefault();
            return ent ?? await CreateEntityAsync(entityName);
        }
    }
}
