using Agitporp.Scraper.Sinks.Newsfeed.Interfaces;
using Agitprop.Core;
using Agitprop.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agitporp.Scraper.Sinks.Newsfeed;

public class NewsfeedSink : ISink
{

    INamedEntityRecognizer NerService;
    INewsfeedDB DataBase;
    ILogger<NewsfeedSink> Logger;

    public NewsfeedSink(INamedEntityRecognizer nerService, INewsfeedDB dataBase, ILogger<NewsfeedSink> logger)
    {
        NerService = nerService;
        DataBase = dataBase;
        Logger = logger;
    }

    public async Task<bool> CheckPageAlreadyVisited(string url)
    {
        return await DataBase.IsUrlAlreadyExists(url);
    }

    public void Emit(string url, List<ContentParserResult> data, CancellationToken cancellationToken = default)
    {
        foreach (var article in data)
        {
            var entities = NerService.AnalyzeSingleAsync(article.Text).Result;
            Logger.LogInformation($"Recieved named entitees for {url}");
            var count = DataBase.CreateMentionsAsync(url, article, entities).Result;
            Logger.LogInformation($"Inserted {count} mentions for {url}");
        }
    }

    public async Task EmitAsync(string url, List<ContentParserResult> data, CancellationToken cancellationToken = default)
    {
        foreach (var article in data)
        {
            var entities = await NerService.AnalyzeSingleAsync(article.Text);
            Logger.LogInformation($"Recieved named entitees for {url}");
            var count = await DataBase.CreateMentionsAsync(url, article, entities);
            Logger.LogInformation($"Inserted {count} mentions for {url}");
        }
    }
}
