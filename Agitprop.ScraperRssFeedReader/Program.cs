using MassTransit;

namespace Agitprop.RssFeedReader;

/// <summary>
/// The entry point for the RssFeedReader application.
/// </summary>
public class Program
{
    /// <summary>
    /// The main method that configures and runs the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.AddServiceDefaults();
        builder.Services.AddHostedService<RssFeedReader>();
        builder.Services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.SetInMemorySagaRepositoryProvider();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(builder.Configuration.GetConnectionString("messaging"));

                cfg.ClearSerialization();
                cfg.AddRawJsonSerializer();
                cfg.ConfigureEndpoints(context);
            });
        });

        var app = builder.Build();
        app.Run();
    }

    /// <summary>
    /// Creates and configures the host builder for the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>An instance of <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<RssFeedReader>();

            });
}
