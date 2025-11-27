using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using Agitprop.Consumer.Consumers;
using Agitprop.Scraper.Consumer.Consumers;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Metrics;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.Process;
using OpenTelemetry.Instrumentation.Http;

using Polly;
using Polly.Retry;

using PuppeteerSharp;

namespace Agitprop.Scraper.Consumer;

/// <summary>
/// Provides extension methods for configuring application services.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Configures MassTransit for message-based communication in the application.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The updated host application builder.</returns>
    public static IHostApplicationBuilder ConfigureMassTransit(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("messaging");
        builder.Services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.SetInMemorySagaRepositoryProvider();
            var entryAssembly = Assembly.GetEntryAssembly();
            x.AddConsumer<NewsfeedJobConsumer, NewsfeedJobConsumerDefinition>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(connectionString, h =>
                {
                    h.Heartbeat(TimeSpan.FromSeconds(20));
                });

                cfg.ClearSerialization();
                cfg.AddRawJsonSerializer();
                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }

    /// <summary>
    /// Configures resiliency pipelines for handling transient errors in the application.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The updated host application builder.</returns>
    public static IHostApplicationBuilder ConfigureResiliency(this IHostApplicationBuilder builder)
    {
        builder.Services.AddResiliencePipeline("Spider", static builder =>
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
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                MaxRetryAttempts = 15,
                UseJitter = false,
            });
        });
        builder.Services.AddResilienceEnricher();
        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry metrics for Aspire observability.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The updated host application builder.</returns>
    public static IHostApplicationBuilder ConfigureMetrics(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                // Built-in instrumentation for metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                
                // Custom spider performance metrics
                .AddMeter("Agitprop.Spider")
                
                // Configure histogram buckets for better analysis
                .AddView("spider.page.load.time", new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = new[] { 100.0, 500.0, 1000.0, 2000.0, 5000.0, 10000.0, 30000.0, 60000.0 }
                })
                .AddView("spider.processing.time", new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = new[] { 500.0, 1000.0, 2000.0, 5000.0, 10000.0, 20000.0, 60000.0 }
                }))
            
            // Add tracing for complete observability
            .WithTracing(tracing => tracing
                .AddSource("Agitprop.Spider"));

        return builder;
    }
}
