using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Polly;
using Polly.Retry;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using PuppeteerSharp;
using Microsoft.Extensions.DependencyInjection;
using NReco.Logging.File;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.SurrealDB;
using Agitprop.Infrastructure;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Core;
using Agitprop.Infrastructure.PageLoader;
using Agitprop.Infrastructure.PageRequester;
using Agitprop.Infrastructure.InMemory;
using Agitprop.Infrastructure.Puppeteer;
using VisitedLinkTracker = Agitprop.Infrastructure.SurrealDB.VisitedLinkTracker;
using Agitprop.Scrapers.Factories;

namespace Agitprop.Consumer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilder, config) =>
            {
                config.AddJsonFile("appsettings.json", false, true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();
                    // By default, sagas are in-memory, but should be changed to a durable
                    // saga repository.
                    x.SetInMemorySagaRepositoryProvider();

                    var entryAssembly = Assembly.GetEntryAssembly();

                    x.AddConsumers(entryAssembly);
                    x.AddSagaStateMachines(entryAssembly);
                    x.AddSagas(entryAssembly);
                    x.AddActivities(entryAssembly);

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost", "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ClearSerialization();
                        cfg.AddRawJsonSerializer();
                        cfg.ConfigureEndpoints(context);
                    });
                });

                services.AddResiliencePipeline("Spider", static builder =>
                {
                    builder.AddRetry(new RetryStrategyOptions
                    {
                        ShouldHandle = args => args.Outcome switch
                        {
                            { Exception: HttpRequestException } => PredicateResult.True(),
                            { Exception: TaskCanceledException } => PredicateResult.True(),
                            { Exception: TimeoutException } => PredicateResult.True(),
                            { Exception: NavigationException } => PredicateResult.True(), // You can handle multiple exceptions
                            { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True(),
                            _ => PredicateResult.False()
                        },
                        BackoffType = DelayBackoffType.Constant,
                        Delay = TimeSpan.FromSeconds(0.2),
                        MaxRetryAttempts = 9,
                        UseJitter = false,
                    });
                });

                services.AddLogging(builder =>
                {
                    builder.AddFile($"..\\logs\\{DateTime.Now:yyyy-mm-dd_HH-dd-ss}_agitprop.log");

                });

                services.AddHostedService<RssFeedReader>();
                services.AddSurreal(hostContext.Configuration.GetConnectionString("SurrealDB") ?? throw new MissingConfigurationValueException("Missing config for SurrealDB"));

                services.AddTransient<IAgitpropDataBaseService, AgitpropDBService>();
                services.AddTransient<INamedEntityRecognizer, NamedEntityRecognizer>();
                services.AddTransient<ISink, AgitpropSink>();

                services.AddTransient<ISpider, Spider>();
                services.AddTransient<ILinkTracker, VisitedLinkTracker>();
                services.AddSingleton<ScrapingJobFactory>();

                services.AddTransient<IBrowserPageLoader, PuppeteerPageLoader>();
                services.AddTransient<ICookiesStorage, CookieStorage>();

                services.AddTransient<IStaticPageLoader, HttpStaticPageLoader>();
                //services.AddTransient<IPageRequester, PageRequester>();
                services.AddTransient<IPageRequester, RotatingProxyPageRequester>();
                services.AddTransient<IProxyProvider, ProxyScrapeProxyProvider>();
            });
    }
}
