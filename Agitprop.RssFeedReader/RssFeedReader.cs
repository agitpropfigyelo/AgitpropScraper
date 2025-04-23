using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

using Agitprop.Scraper.Sinks.Newsfeed;
using Agitprop.Core.Enums;

using MassTransit;

namespace Agitprop.RssFeedReader;

public class RssFeedReader : IHostedService, IDisposable
{
    private Timer? _timer;
    private string[] _feeds;
    private ILogger<RssFeedReader> _logger;
    private IServiceScopeFactory _scopeFactory;
    private TimeSpan _interval;

    public RssFeedReader(IConfiguration configuration, ILogger<RssFeedReader> logger, IServiceScopeFactory scopeFactory)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _logger = logger;
        _feeds = configuration.GetSection("RssFeedReader:Feeds").Get<string[]>()??throw new ArgumentException("Feeds are not defined");
        _scopeFactory = scopeFactory;
        _interval = TimeSpan.FromMinutes(configuration.GetValue<double>("RssFeedReader:IntervalMinutes", 60));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, _interval);
        return Task.CompletedTask;
    }

    private void ExecuteTask(object? state)
    {
        _logger.LogInformation("Running RSS readers");
        using var scope = _scopeFactory.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var jobs = FetchScrapingJobs();
        publishEndpoint.PublishBatch(jobs).Wait();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private List<NewsfeedJobDescrpition> FetchScrapingJobs()
    {
        var scrapingJobs = new List<NewsfeedJobDescrpition>();

        foreach (var feedUrl in _feeds)
        {
            _logger.LogDebug("Reading RSS feed: {feedUrl}",feedUrl);
            try
            {
                using var reader = XmlReader.Create(feedUrl);
                var feed = SyndicationFeed.Load(reader);

                if (feed != null)
                {
                    var news = feed.Items.Select(item => new NewsfeedJobDescrpition
                    {
                        Url = item.Links.FirstOrDefault()?.Uri.GetLeftPart(UriPartial.Path)?? throw new ArgumentException("No link found"),
                        Type = PageContentType.Article // Assuming all RSS feed items are articles
                    });
                    _logger.LogDebug("New articles: {newList}",news); //TODO: improve logging with stringyfied list
                    scrapingJobs.AddRange(news);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing feed {feedUrl}: {msg}",feedUrl,ex.Message);
            }
        }

        return scrapingJobs;
    }
}
