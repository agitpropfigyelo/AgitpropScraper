using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using Agitporp.Scraper.Sinks.Newsfeed;

using Agitprop.Infrastructure.Puppeteer;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NReco.Logging.File;

using Polly;
using Polly.Retry;

using PuppeteerSharp;

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
            .ConfigureAppConfiguration(ConfigureApp)
            .ConfigureServices(ConfigureServices);

        private static void ConfigureApp(HostBuilderContext hostBuilder, IConfigurationBuilder config)
        {
            config.AddJsonFile("appsettings.json", false, true);
            if (hostBuilder.HostingEnvironment.IsDevelopment())
            {
                Console.WriteLine("Development environment detected. Loading appsettings.Development.json");
                config.AddJsonFile("appsettings.development.json", optional: false, reloadOnChange: true);
            }
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.SetInMemorySagaRepositoryProvider();
                var entryAssembly = Assembly.GetEntryAssembly();
                x.AddConsumers(entryAssembly);
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(hostContext.Configuration.GetValue<string>("Infrastructure:RabbitMQ"), "/", h =>
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
                        { Exception: NavigationException } => PredicateResult.True(),
                        { Result: HttpResponseMessage response } when !response.IsSuccessStatusCode => PredicateResult.True(),
                        _ => PredicateResult.False()
                    },
                    BackoffType = DelayBackoffType.Constant,
                    Delay = TimeSpan.FromSeconds(0.2),
                    MaxRetryAttempts = 9,
                    UseJitter = false,
                });
            });

            services.AddLogging(ConfigureLogging);
            services.AddHostedService<RssFeedReader>();
            //services.AddSurreal(hostContext.Configuration.GetConnectionString("SurrealDB") ?? throw new MissingConfigurationValueException("Missing config for SurrealDB"));
            services.AddNewsfeedSink(hostContext.Configuration);
            services.ConfigureInfrastructureWithBrowser();
        }

        private static void ConfigureLogging(ILoggingBuilder builder)
        {
            builder.AddFile($"..\\logs\\{DateTime.Now:yyyy-mm-dd_HH-dd-ss}_agitprop.log");
            builder.AddConsole();
        }
    }
}
