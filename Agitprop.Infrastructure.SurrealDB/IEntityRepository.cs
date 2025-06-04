using Agitprop.Infrastructure.SurrealDB.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agitprop.Infrastructure.SurrealDB;

public interface IEntityRepository
{
    /// <summary>
    /// Gets all entities, optionally filtered by a fuzzy substring query (case-insensitive).
    /// </summary>
    /// <param name="query">Optional substring or fuzzy search for entity name.</param>
    /// <returns>List of entities.</returns>
    Task<IEnumerable<Entity>> GetEntitiesAsync(string? query = null);

    /// <summary>
    /// Gets all mentions for a given entity within a date range.
    /// </summary>
    /// <param name="entityId">Entity id.</param>
    /// <param name="from">Start date (inclusive).</param>
    /// <param name="to">End date (inclusive).</param>
    /// <returns>List of mentions with article info.</returns>
    Task<IEnumerable<(Article Article, Mentions Mention)>> GetMentionsAsync(string entityId, DateTime from, DateTime to);

    /// <summary>
    /// Gets an entity by id.
    /// </summary>
    /// <param name="entityId">Entity id.</param>
    /// <returns>Entity or null.</returns>
    Task<Entity?> GetEntityByIdAsync(string entityId);
    Task<IEnumerable<(DateTime date, int count)>> GetTrendingMentionsAsync(string entityId, DateTime from, DateTime to);
}
