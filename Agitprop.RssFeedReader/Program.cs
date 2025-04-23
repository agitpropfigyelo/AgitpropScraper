using MassTransit;

namespace Agitprop.RssFeedReader;

public class Program
{
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

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<RssFeedReader>();

            });
}
