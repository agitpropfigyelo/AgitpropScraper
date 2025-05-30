﻿using SurrealDb.Net.Models;

namespace Agitprop.Scraper.Sinks.Newsfeed.Database.Models;

/// <summary>
/// Represents an entity that can be mentioned in articles.
/// </summary>
internal class Entity : Record
{
    /// <summary>
    /// Gets or sets the name of the entity.
    /// </summary>
    public string Name { get; set; }
}
