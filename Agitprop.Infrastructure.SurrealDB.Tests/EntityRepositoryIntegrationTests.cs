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
    public void OneTimeSetup()
    {
        _client = new SurrealDbMemoryClient();
        // Initialize the repository with the SurrealDB client
    }

    [OneTimeTearDownAttribute]
    public void OneTimeTearDown()
    {
        // Dispose of the client if necessary
        _client?.Dispose();
    }

    [SetUp]
    public async Task Setup()
    {
        _repository = new EntityRepository(_client, new NullLogger<EntityRepository>());
        Assert.That(_repository, Is.Not.Null);

    }

    [TearDown]
    public async Task TearDown()
    {
        if (_client != null)
            await _client.DisposeAsync();
    }

    [Test]
    public async Task GetEntitiesAsync_ReturnsAllEntities()
    {
        var entities = await _repository.GetEntitiesAsync();
        Assert.That(entities, Is.Not.Empty);
    }

    [Test]
    public async Task GetEntitiesAsync_FuzzySearch_Works()
    {
        var entities = await _repository.GetEntitiesAsync("test");
        Assert.That(entities.Any(e => e.Name.Contains("Test", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public async Task GetEntityByIdAsync_ReturnsEntity()
    {
        var all = await _repository.GetEntitiesAsync();
        var first = all.FirstOrDefault();
        Assert.That(first, Is.Not.Null);
        var entity = await _repository.GetEntityByIdAsync(first!.Id.ToString());
        Assert.That(entity, Is.Not.Null);
        Assert.That(entity!.Name, Is.EqualTo(first.Name));
    }

    [Test]
    public async Task GetMentionsAsync_ReturnsMentions()
    {
        Assert.Ignore("SurrealDB test instance is not configured. Please provide a running SurrealDB for integration tests.");
    }
}
