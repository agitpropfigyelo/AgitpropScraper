using Agitprop.Scraper.Sinks.Newsfeed.Interfaces;

using Agitprop.Core;
using Agitprop.Infrastructure.Interfaces;

using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Agitprop.Scraper.Sinks.Newsfeed;

public class NewsfeedSink : ISink
{

    INamedEntityRecognizer NerService;
    INewsfeedDB DataBase;
    ILogger<NewsfeedSink> Logger;
    private ActivitySource ActivitySource = new("Agitprop.Sink.Newsfeed");


    public NewsfeedSink(INamedEntityRecognizer nerService, INewsfeedDB dataBase, ILogger<NewsfeedSink> logger)
    {
        NerService = nerService;
        DataBase = dataBase;
        Logger = logger;
    }

    public async Task<bool> CheckPageAlreadyVisited(string url)
    {
        using var trace = this.ActivitySource.StartActivity("CheckPageAlreadyVisited");
        return await DataBase.IsUrlAlreadyExists(url);
    }

    public async Task EmitAsync(string url, List<ContentParserResult> data, CancellationToken cancellationToken = default)
    {
        using var trace = this.ActivitySource.StartActivity("EmitAsync");
        foreach (var article in data)
        {
            var entities = await NerService.AnalyzeSingleAsync(article.Text);
            Logger.LogInformation("Recieved named entitees for {url}", url);
            var count = await DataBase.CreateMentionsAsync(url, article, entities);
            Logger.LogInformation("Inserted {count} mentions for {url}", count, url);
        }
    }
}
