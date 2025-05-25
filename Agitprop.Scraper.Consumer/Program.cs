using Agitprop.Infrastructure.Puppeteer;
using Agitprop.Sinks.Newsfeed;

using Microsoft.Extensions.Hosting;

namespace Agitprop.Scraper.Consumer;

/// <summary>
/// The entry point for the Agitprop Consumer application.
/// </summary>
public class Program
{
    /// <summary>
    /// The main method that configures and runs the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure infrastructure with browser support.
        builder.ConfigureInfrastructureWithBrowser();

        // Configure MassTransit for message-based communication.
        builder.ConfigureMassTransit();
        builder.ConfigureResiliency();

        // Add the Newsfeed sink for processing scraped data.
        builder.AddNewsfeedSink();

        // Add default service configurations.
        builder.AddServiceDefaults();

        var app = builder.Build();
        app.Run();
    }
}