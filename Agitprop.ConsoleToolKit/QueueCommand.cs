using System;
using System.CommandLine;
using System.Text;
using System.Text.Json;

using Agitprop.Core.Enums;

using RabbitMQ.Client;

namespace Agitprop.ConsoleToolKit;

public static class QueueCommand
{
    private static readonly string QueueName = "scraping-job";
    private static readonly string CommandName = "queue";

    internal static Command AddQueueCommand(this RootCommand rootCommand)
    {
        var dateOption = new Option<DateOnly>(
            ["--date", "-d"],
            () => DateOnly.FromDateTime(DateTime.Today - TimeSpan.FromDays(1)),
            "Specifies the date for the scraping job descriptions");
        var siteOption = new Option<NewsSites[]>(
            ["--site", "-s"],
            Enum.GetValues<NewsSites>,
            "Specifies the news sites to include (comma-separated or multiple flags)");

        var addCommand = new Command(CommandName, "Publishes scraping jobs to queue")
        {
            dateOption,
            siteOption,
        };
        addCommand.SetHandler(PublishJob, dateOption, siteOption);

        rootCommand.Add(addCommand);
        return addCommand;
    }

    private static async Task PublishJob(DateOnly date, NewsSites[] sites)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: QueueName,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

        foreach (var site in sites)
        {
            //TODO: fix this
            var message = JsonSerializer.Serialize("job");
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchange: "",
                                            routingKey: QueueName,
                                            mandatory: true,
                                            body: body);

            Console.WriteLine($"Published job: {message}");
        }

        await Task.CompletedTask;
    }
}
