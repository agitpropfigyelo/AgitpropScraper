using System.Diagnostics;

using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.SurrealDB;

using Microsoft.Extensions.Logging;

namespace Agitprop.Sinks.Newsfeed;

/// <summary>
/// Represents a sink for processing and storing newsfeed data.
/// </summary>
public class NewsfeedSink : ISink
{
    private INamedEntityRecognizer NerService;
    private INewsfeedDB DataBase;
    private ILogger<NewsfeedSink> Logger;
    private ActivitySource ActivitySource = new("Agitprop.Sink.Newsfeed");

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsfeedSink"/> class.
    /// </summary>
    /// <param name="nerService">The named entity recognizer service.</param>
    /// <param name="dataBase">The database for storing newsfeed data.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    public NewsfeedSink(INamedEntityRecognizer nerService, INewsfeedDB dataBase, ILogger<NewsfeedSink> logger)
    {
        NerService = nerService;
        DataBase = dataBase;
        Logger = logger;
    }

    /// <summary>
    /// Checks if a page with the specified URL has already been visited.
    /// </summary>
    /// <param name="url">The URL of the page to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the page has been visited.</returns>
    public async Task<bool> CheckPageAlreadyVisited(string url)
    {
        using var trace = ActivitySource.StartActivity("CheckPageAlreadyVisited");
        return await DataBase.IsUrlAlreadyExists(url);
    }

    /// <summary>
    /// Processes and stores the parsed content data for a specific URL.
    /// </summary>
    /// <param name="url">The URL of the page being processed.</param>
    /// <param name="data">The list of parsed content results.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task EmitAsync(string url, List<ContentParserResult> data, CancellationToken cancellationToken = default)
    {
        using var trace = ActivitySource.StartActivity("EmitAsync");
        foreach (var article in data)
        {
            var entities = await NerService.AnalyzeSingleAsync(article.Text);
            Logger.LogInformation("Recieved named entitees for {url}", url);
            var count = await DataBase.CreateMentionsAsync(url, article, entities);
            Logger.LogInformation("Inserted {count} mentions for {url}", count, url);
        }
    }
}
