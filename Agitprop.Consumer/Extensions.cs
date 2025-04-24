using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Polly;
using Polly.Retry;

using PuppeteerSharp;

namespace Agitprop.Consumer;

public static class Extensions
{

    public static IHostApplicationBuilder ConfigureMassTransit(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.SetInMemorySagaRepositoryProvider();
            var entryAssembly = Assembly.GetEntryAssembly();
            x.AddConsumers(entryAssembly);
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(builder.Configuration.GetConnectionString("messaging"));

                cfg.ClearSerialization();
                cfg.AddRawJsonSerializer();
                cfg.ConfigureEndpoints(context);
            });
        });

        return builder;
    }

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
                        BackoffType = DelayBackoffType.Constant,
                        Delay = TimeSpan.FromSeconds(0.2),
                        MaxRetryAttempts = 9,
                        UseJitter = false,
                    });
                });

        return builder;
    }
}
