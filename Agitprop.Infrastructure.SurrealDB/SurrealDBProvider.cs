using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.SurrealDB.Models;
using Microsoft.Extensions.Logging;
using SurrealDb.Net;
using SurrealDb.Net.Exceptions;
using SurrealDb.Net.Models.Auth;
using SurrealDb.Net.Models.Response;

namespace Agitprop.Infrastructure
{
    /*TODO:
    átírni a sémát: source -publish-> article -mentions->entity
    a létrejött article date-jét az entity-k beszúrása előtt frissítjük
    */
    public class SurrealDBProvider
    {
        private readonly static string relateQuery = "RELATE $src->mentions->$eid SET Date=$d, Url=$u;";
        private readonly static string articleExistsQuery = "select * from mentions where Url=$en";

        private readonly static string endpoint = "ws://127.0.0.1:8000/rpc";
        private readonly static string ns = "agitprop";
        private readonly static string dbName = "test";
        private readonly static string userName = "root";
        private readonly static string userPassword = "root";
        protected ILogger Logger;

        public Task Initialization => Initialize();
        private bool beenInitialized = false;

        public SurrealDBProvider(ILogger logger)
        {
            Logger = logger;
        }

        public async Task Initialize()
        {
            if (beenInitialized) return;
            var idk = AppDomain.CurrentDomain.BaseDirectory;
            var client = await CreateClientAsync();
            await client.RawQuery(File.ReadAllText(Path.Combine(idk, "db_init.surql")));
            beenInitialized = true;
        }

        protected async Task<SurrealDbClient> CreateClientAsync()
        {
            var db = new SurrealDbClient(endpoint);

            await db.SignIn(new RootAuth { Username = userName, Password = userPassword });
            await db.Use(ns, dbName);
            return db;
        }

    }
}
