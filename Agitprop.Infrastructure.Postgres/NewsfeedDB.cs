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

            // Check if article already exists and get it if it does
            var existingArticle = await _db.Articles
                .Include(a => a.Mentions)
                .FirstOrDefaultAsync(a => a.Url == url);

            PostgresArticle articleToUse;
            bool isUpdate = false;

            if (existingArticle != null)
            {
                _logger.LogInformation("Article already exists in DB: {@Url}", url);
                articleToUse = existingArticle;
                isUpdate = true;

                // Update article fields
                articleToUse.Title = article.Title;
                articleToUse.PublishedTime = DateTime.SpecifyKind(article.PublishDate, DateTimeKind.Utc);

                // Remove existing mentions to recreate them
                _db.Mentions.RemoveRange(existingArticle.Mentions);
            }
            else
            {
                _logger.LogInformation("Article does not exist in DB: {@Url}", url);
                articleToUse = new PostgresArticle
                {
                    Id = Guid.NewGuid(),
                    Title = article.Title,
                    Url = url,
                    PublishedTime = DateTime.SpecifyKind(article.PublishDate, DateTimeKind.Utc)
                };
                _db.Articles.Add(articleToUse);
            }

            // var newArticle = new PostgresArticle
            // {
            //     Id = Guid.NewGuid(),
            //     Title = article.Title,
            //     Url = url,
            //     PublishedTime = DateTime.SpecifyKind(article.PublishDate, DateTimeKind.Utc)
            // };
            // _db.Articles.Add(newArticle);

            int addedEntities = 0;

            foreach (var entity in entities.All.Distinct())
            {
                using var entityActivity = _activitySource.StartActivity("ProcessEntity");
                entityActivity?.SetTag("entity.name", entity);

                var dbEntity = await _db.Entities.FirstOrDefaultAsync(e => e.Name == entity.Name);

                if (dbEntity == null)
                {
                    dbEntity = new PostgresEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = entity.Name,
                        Type = entity.Type
                    };
                    _db.Entities.Add(dbEntity);
                    addedEntities++;
                    _logger.LogDebug("Added new entity {@EntityName} with Id {@EntityId}", entity, dbEntity.Id);
                }

                _db.Mentions.Add(new PostgresMention
                {
                    Id = Guid.NewGuid(),
                    Article = articleToUse,
                    Entity = dbEntity
                });
                _logger.LogDebug(
                    isUpdate
                        ? "Updated mention for entity {@EntityName} in article {@ArticleUrl}"
                        : "Created mention for entity {@EntityName} in article {@ArticleUrl}",
                    entity, url);

                entityActivity?.SetStatus(ActivityStatusCode.Ok);
            }

            int changes = await _db.SaveChangesAsync();

            _logger.LogInformation(
                isUpdate 
                    ? "Finished updating mentions. Total entities added: {EntitiesAdded}, DB changes: {DbChanges}"
                    : "Finished creating mentions. Total entities added: {EntitiesAdded}, DB changes: {DbChanges}",
                addedEntities, changes);

            activity?.SetStatus(ActivityStatusCode.Ok, isUpdate ? "Mentions updated" : "Mentions created");
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
