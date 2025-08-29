using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Agitprop.Core.Enums;

using MassTransit;
using System.Diagnostics;
using Agitprop.Sinks.Newsfeed;

namespace Agitprop.Scraper.RssFeedReader;

/// <summary>
/// A hosted service that reads RSS feeds and publishes scraping jobs.
/// </summary>
public class RssFeedReader : IHostedService, IDisposable
{
    private Timer? _timer;
    private string[] _feeds;
    private ILogger<RssFeedReader> _logger;
    private IServiceScopeFactory _scopeFactory;
    private TimeSpan _interval;
    private ActivitySource ActivitySource = new("Agitprop.RssFeedReader");

    /// <summary>
    /// Initializes a new instance of the <see cref="RssFeedReader"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    /// <param name="scopeFactory">The service scope factory for creating service scopes.</param>
    public RssFeedReader(IConfiguration configuration, ILogger<RssFeedReader> logger, IServiceScopeFactory scopeFactory)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _logger = logger;
        _feeds = configuration.GetSection("Feeds").Get<string[]>() ?? throw new ArgumentException("Feeds are not defined");
        _scopeFactory = scopeFactory;
        _interval = TimeSpan.FromMinutes(configuration.GetValue<double>("IntervalMinutes", 60));
    }

    /// <summary>
    /// Starts the hosted service.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A completed task.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var trace = ActivitySource.StartActivity("StartAsync");
        _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, _interval);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the task to fetch and publish scraping jobs.
    /// </summary>
    /// <param name="state">The state object passed to the timer.</param>
    private void ExecuteTask(object? state)
    {
        using var trace = ActivitySource.StartActivity("ExecuteTask");
        _logger.LogInformation("Running RSS readers");
        using var scope = _scopeFactory.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var jobs = FetchScrapingJobs();
        publishEndpoint.PublishBatch(jobs).Wait();
    }

    /// <summary>
    /// Stops the hosted service.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        using var trace = ActivitySource.StartActivity("StopAsync");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the resources used by the service.
    /// </summary>
    public void Dispose()
    {
        _timer?.Dispose();
    }

    /// <summary>
    /// Fetches scraping jobs from the configured RSS feeds.
    /// </summary>
    /// <returns>A list of <see cref="NewsfeedJobDescrpition"/> objects representing the scraping jobs.</returns>
    private List<NewsfeedJobDescrpition> FetchScrapingJobs()
    {
        using var trace = ActivitySource.StartActivity("FetchScrapingJobs", ActivityKind.Producer);

        var scrapingJobs = new List<NewsfeedJobDescrpition>();

        foreach (var feedUrl in _feeds)
        {
            _logger.LogDebug("Reading RSS feed: {feedUrl}", feedUrl);
            try
            {
                using var reader = XmlReader.Create(feedUrl);
                var feed = SyndicationFeed.Load(reader);

                if (feed != null)
                {
                    var news = feed.Items.Select(item => new NewsfeedJobDescrpition
                    {
                        Url = item.Links.FirstOrDefault()?.Uri.GetLeftPart(UriPartial.Path) ?? throw new ArgumentException("No link found"),
                        Type = PageContentType.Article // Assuming all RSS feed items are articles
                    });
                    _logger.LogDebug("New articles: {newList}", news); //TODO: improve logging with stringyfied list
                    scrapingJobs.AddRange(news);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing feed {feedUrl}: {msg}", feedUrl, ex.Message);
            }
        }

        return scrapingJobs;
    }
}