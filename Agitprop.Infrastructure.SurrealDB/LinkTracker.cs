using System;
using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.SurrealDB.Models;
using Microsoft.Extensions.Logging;
using SurrealDb.Net.Exceptions;

namespace Agitprop.Infrastructure.SurrealDB
{
    public class VisitedLinkTracker : SurrealDBProvider, ILinkTracker
    {
        private const string visitedLinksTable = "visitedLinks";


        public VisitedLinkTracker(ILogger logger) : base(logger)
        {
        }

        public async Task AddVisitedLinkAsync(string visitedLink)
        {
            try
            {
                var client = await CreateClientAsync();
                var vs = new VisitedLink { Link = visitedLink };
                await client.Create(visitedLinksTable, vs);
                Logger.LogInformation($"Added visited link {visitedLink}");
            }
            catch (SurrealDbException ex)
            {
                Logger.LogWarning(ex, $"Failed to add visited link {visitedLink}; ");
                throw new PageAlreadyVisitedException();
            }
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

        public Task<List<string>> GetVisitedLinksAsync()
        {
            throw new NotImplementedException();
        }
        public async Task<long> GetVisitedLinksCount()
        {
            var str = "return count(Select * FROM visitedLinks);";
            var client = await CreateClientAsync();
            var result = await client.RawQuery(str);
            return result.FirstOk.GetValue<long>();
        }
    }
}
