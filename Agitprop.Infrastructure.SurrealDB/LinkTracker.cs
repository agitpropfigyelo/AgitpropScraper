using System;
using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.SurrealDB.Models;
using Microsoft.Extensions.Logging;
using SurrealDb.Net;
using SurrealDb.Net.Exceptions;

namespace Agitprop.Infrastructure.SurrealDB
{
    public class VisitedLinkTracker : ILinkTracker
    {
        private const string visitedLinksTable = "visitedLinks";
        private ILogger<VisitedLinkTracker> logger;
        private ISurrealDbClient client;

        public VisitedLinkTracker(ILogger<VisitedLinkTracker> logger, ISurrealDbClient client)
        {
            this.logger = logger;
            this.client = client;
        }

        public async Task AddVisitedLinkAsync(string visitedLink)
        {
            try
            {
                var vs = new VisitedLink { Link = visitedLink };
                await client.Create(visitedLinksTable, vs, default);
                logger.LogInformation($"Added visited link {visitedLink}");
            }
            catch (SurrealDbException ex)
            {
                logger.LogWarning(ex, $"Failed to add visited link {visitedLink}; ");
                throw new PageAlreadyVisitedException();
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "MI A FASZ");
            }
        }


        public async Task<List<string>> GetNotVisitedLinks(IEnumerable<string> links)
        {
            try
            {
                FormattableString str = $"return array::complement([{string.Join(',', links.Select(x => $"'Name':'{x}'"))}],(SELECT Link FROM visitedLinks));";
                var idk = await client.Query(str);
                return [];
            }
            catch (System.Exception ex)
            {
                logger.LogError($"Failed to qurey EX: {ex.Message}");
            }
            return [];
        }

        public Task<List<string>> GetVisitedLinksAsync()
        {
            throw new NotImplementedException();
        }
        public async Task<long> GetVisitedLinksCount()
        {
            var str = "return count(Select * FROM visitedLinks);";
            var result = await client.RawQuery(str);
            return result.FirstOk.GetValue<long>();
        }
    }
}
