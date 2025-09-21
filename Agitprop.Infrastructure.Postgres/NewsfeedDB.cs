using System;

using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Postgres.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure.Postgres;

public class NewsfeedDB(AppDbContext db, ILogger<NewsfeedDB> logger) : INewsfeedDB
{
    private readonly AppDbContext _db = db;
    private ILogger<NewsfeedDB> _logger = logger;

     public async Task<int> CreateMentionsAsync(
        string url,
        ContentParserResult article,
        NamedEntityCollection entities)
    {
        // Check if article already exists
        var exists = await _db.Articles.AnyAsync(a => a.Url == url);
        if (exists)
            return 0;

        // Create Article
        var newArticle = new PostgresArticle
        {
            Id = Guid.NewGuid(),
            Url = url,
            PublishedTime = article.PublishDate
        };

        // Attach Entities + Mentions
        foreach (var name in entities.All.Distinct()) // avoid duplicate mentions
        {
            var dbEntity = await _db.Entities
                .FirstOrDefaultAsync(e => e.Name == name);

            if (dbEntity == null)
            {
                dbEntity = new PostgresEntity
                {
                    Id = Guid.NewGuid(),
                    Name = name
                };
                _db.Entities.Add(dbEntity);
            }

            newArticle.Mentions.Add(new PostgresMention
            {
                Id = Guid.NewGuid(),
                Article = newArticle,
                Entity = dbEntity
            });
        }

        _db.Articles.Add(newArticle);

        return await _db.SaveChangesAsync();
    }

    public async Task<bool> IsUrlAlreadyExists(string url)
    {
        return await _db.Articles.AnyAsync(a => a.Url == url);
    }
}