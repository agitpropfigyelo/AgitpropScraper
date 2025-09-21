using System;
using System.Diagnostics;
using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure.Postgres;

public class NewsfeedDB(AppDbContext db, ILogger<NewsfeedDB> logger) : INewsfeedDB
{
    private readonly AppDbContext _db = db;
    private readonly ILogger<NewsfeedDB> _logger = logger;
    private static readonly ActivitySource _activitySource = new("Agitprop.NewsfeedDB");

    public async Task<int> CreateMentionsAsync(
        string url,
        ContentParserResult article,
        NamedEntityCollection entities)
    {
        using var activity = _activitySource.StartActivity("CreateMentionsAsync");
        activity?.SetTag("article.url", url);
        activity?.SetTag("article.source", article.SourceSite.ToString());
        activity?.SetTag("article.publishDate", article.PublishDate);

        try
        {
            _logger.LogInformation("Creating mentions for article {@Url} from {@Source} at {@PublishDate}",
                url, article.SourceSite, article.PublishDate);

            // Check if article already exists
            if (await _db.Articles.AnyAsync(a => a.Url == url))
            {
                _logger.LogInformation("Article already exists in DB: {@Url}", url);
                activity?.SetStatus(ActivityStatusCode.Ok, "Article exists");
                return 0;
            }

            var newArticle = new PostgresArticle
            {
                Id = Guid.NewGuid(),
                Url = url,
                PublishedTime = article.PublishDate
            };

            int addedEntities = 0;

            foreach (var name in entities.All.Distinct())
            {
                using var entityActivity = _activitySource.StartActivity("ProcessEntity");
                entityActivity?.SetTag("entity.name", name);

                var dbEntity = await _db.Entities.FirstOrDefaultAsync(e => e.Name == name);

                if (dbEntity == null)
                {
                    dbEntity = new PostgresEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = name
                    };
                    _db.Entities.Add(dbEntity);
                    addedEntities++;
                    _logger.LogDebug("Added new entity {@EntityName} with Id {@EntityId}", name, dbEntity.Id);
                }

                newArticle.Mentions.Add(new PostgresMention
                {
                    Id = Guid.NewGuid(),
                    Article = newArticle,
                    Entity = dbEntity
                });
                _logger.LogDebug("Created mention for entity {@EntityName} in article {@ArticleUrl}", name, url);

                entityActivity?.SetStatus(ActivityStatusCode.Ok);
            }

            _db.Articles.Add(newArticle);
            int changes = await _db.SaveChangesAsync();

            _logger.LogInformation("Finished creating mentions. Total entities added: {EntitiesAdded}, DB changes: {DbChanges}",
                addedEntities, changes);

            activity?.SetStatus(ActivityStatusCode.Ok, "Mentions created");
            return changes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating mentions for article {@Url} from {@Source} at {@PublishDate}",
                url, article.SourceSite, article.PublishDate);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<bool> IsUrlAlreadyExists(string url)
    {
        using var activity = _activitySource.StartActivity("IsUrlAlreadyExists");
        activity?.SetTag("article.url", url);

        try
        {
            bool exists = await _db.Articles.AnyAsync(a => a.Url == url);
            _logger.LogDebug("Checked existence of article {@Url}: {Exists}", url, exists);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if article exists: {@Url}", url);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
