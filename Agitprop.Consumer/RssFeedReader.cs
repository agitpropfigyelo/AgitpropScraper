using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Agitprop.Core.Contracts;
using Agitprop.Core.Enums;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Agitprop.Consumer;

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
        _feeds = configuration.GetSection("RssFeedReader:Feeds").Get<string[]>();
        _scopeFactory = scopeFactory;
        _interval = TimeSpan.FromMinutes(configuration.GetValue<double>("RssFeedReader:IntervalMinutes", 60));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(ExecuteTask, null, TimeSpan.Zero, _interval);
        return Task.CompletedTask;
    }

    private async void ExecuteTask(object state)
    {
        _logger.LogInformation("Running RSS reader:");
        using var scope = _scopeFactory.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var jobs = FetchScrapingJobs();
        await publishEndpoint.PublishBatch(jobs);
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

    private List<ScrapingJobDescription> FetchScrapingJobs()
    {
        var scrapingJobs = new List<ScrapingJobDescription>();

        foreach (var feedUrl in _feeds)
        {
            _logger.LogDebug($"Reading RSS feed: {feedUrl}");
            try
            {
                using var reader = XmlReader.Create(feedUrl);
                var feed = SyndicationFeed.Load(reader);

                if (feed != null)
                {
                    var news = feed.Items.Select(item => new ScrapingJobDescription
                    {
                        Url = item.Links.FirstOrDefault()?.Uri,
                        Type = PageContentType.Article // Assuming all RSS feed items are articles
                    });
                    _logger.LogDebug($"New articles: {news.ToList()}");
                    scrapingJobs.AddRange(news);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing feed {feedUrl}: {ex.Message}");
            }
        }

        return scrapingJobs;
    }
}
