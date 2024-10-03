using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure;
using Agitprop.Worker;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();
        builder.Configuration.AddJsonFile("appsettings.json", false);
        builder.Services.AddSingleton<IScheduler, Agitprop.Infrastructure.InMemory.Scheduler>();
        builder.Services.AddLogging((builder) =>
        {
            builder.AddConsole();
        });

        var host = builder.Build();
        host.Run();
    }
}