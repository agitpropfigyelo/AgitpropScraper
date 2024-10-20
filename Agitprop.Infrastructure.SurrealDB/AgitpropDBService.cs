using System;
using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.SurrealDB.Models;
using Microsoft.Extensions.Logging;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using SurrealDb.Net.Models.Response;

namespace Agitprop.Infrastructure.SurrealDB
{
    public class AgitpropDBService : IAgitpropDataBaseService
    {
        private readonly static string selectEntityQuery = "select id from entity where Name=$en";
        private ILogger<AgitpropDBService> logger;
        private ISurrealDbClient client;

        public AgitpropDBService(ILogger<AgitpropDBService> logger, ISurrealDbClient client)
        {
            this.logger = logger;
            this.client = client;
        }

        public async Task<int> CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities)
        {
            try
            {
                var src = RecordId.From("source", $"{article.SourceSite}");
                var mention = new Mentions{Date=article.PublishDate,Url=url};
                var entIds = entities.All.Select(e => GetOrAddEntityAsync(e).Result.Id);
                var kdi = await client.Relate<Mentions,Mentions>("mentions",src,entIds, mention);
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
