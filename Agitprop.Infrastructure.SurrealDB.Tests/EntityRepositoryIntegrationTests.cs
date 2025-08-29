using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;

using SurrealDb.Embedded.InMemory;
using SurrealDb.Net;

namespace Agitprop.Infrastructure.SurrealDB.Tests;

[TestFixture]
public class EntityRepositoryIntegrationTests
{
    private ISurrealDbClient _client;
    private EntityRepository _repository;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _client = new SurrealDbMemoryClient();
        // Initialize the repository with the SurrealDB client
        _client.Use("test_db", "test_ns");
        // Run the db_init.surql file to initialize the database
        var dbInitScript = File.ReadAllText("db_init.surql");
        await _client.Import(dbInitScript);

        _repository = new EntityRepository(_client, new NullLogger<EntityRepository>());
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Dispose of the client if necessary
        _client?.Dispose();
    }

    [SetUp]
    public async Task Setup()
    {
        Assert.That(_repository, Is.Not.Null);
    }

    [Test]
    public void GetEntitiesAsync_ReturnsAllEntities()
    {
        var entities = _repository.GetEntitiesAsync().Result;
        var result = entities.ToList(); //have to materialize the async result
        Assert.Multiple(() =>
                {
                    Assert.That(result, Is.Not.Null,"The result should not be null");
                    Assert.That(result, Is.Not.Empty,"The result should not be empty");
                    Assert.That(result, Has.Count.EqualTo(13119),"Contains wrong number of entities");
                });
    }

    [Test]
    public async Task GetEntitiesAsync_FuzzySearch_Works()
    {
        var entities = await _repository.SearchEntitiesAsync("Yves");
        Assert.That(entities.Any(e => e.Name.Contains("Yves", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public async Task GetEntityByIdAsync_ReturnsEntity()
    {
        var all = await _repository.GetEntitiesAsync();
        var first = all.FirstOrDefault();
        Assert.That(first, Is.Not.Null);
        var entity = await _repository.GetEntityByIdAsync(first!.Id.DeserializeId<string>());
        Assert.That(entity, Is.Not.Null);
        Assert.That(entity!.Name, Is.EqualTo(first.Name));
    }

    [Test]
    public async Task GetMentionsAsync_ReturnsMentions()
    {
        var result = await _repository.GetMentioningArticlesAsync("00evx2b5d2aaot6usg2n", DateTime.Parse("2025-06-06T00:00:00Z"), DateTime.Parse("2025-06-28T00:00:00Z"));
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count(), Is.EqualTo(18));
    }
}
