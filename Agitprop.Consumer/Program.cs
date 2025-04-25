using Agitprop.Scraper.Sinks.Newsfeed;

using Agitprop.Infrastructure.Puppeteer;
using Microsoft.Extensions.Hosting;

namespace Agitprop.Consumer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);


        builder.ConfigureInfrastructureWithBrowser();

        builder.ConfigureMassTransit();
        builder.ConfigureResiliency();

        builder.AddNewsfeedSink();

        builder.AddServiceDefaults();
        var app = builder.Build();
        app.Run();
    }
}