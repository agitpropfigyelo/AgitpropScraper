using System;
using System.CommandLine;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;

namespace Agitprop.ConsoleToolKit;

public static class RetryCommand
{
    private static readonly string DeadLetterQueueName = "newsfeed-job_error";
    private static readonly string MainQueueName = "newsfeed-job";
    private static readonly string CommandName = "retry";

    internal static Command AddRetryCommand(this RootCommand rootCommand)
    {
        var prodOption = new Option<bool>(
            ["--prod"],
            () => false,
            "Specifies if the configuration to use is production");

        var listOption = new Option<bool>(
            ["--list"],
            () => false,
            "Lists all messages in the dead-letter queue without removing them");

        var retryCommand = new Command(CommandName, "Retries messages from the dead-letter queue")
        {
            prodOption,
            listOption
        };
        retryCommand.SetHandler((bool prod, bool list) => 
        {
            if (list)
                ListMessages(prod).Wait();
            else
                RetryMessages(prod).Wait();
        }, prodOption, listOption);

        rootCommand.Add(retryCommand);
        return retryCommand;
    }

    private static async Task ListMessages(bool isProd)
    {
        Console.WriteLine($"Using configuration: {(isProd ? "production" : "development")}");
        var configBuilder = new ConfigurationBuilder();
        if (isProd)
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RabbitMQ:Host", "192.172.25.45" },
                    { "RabbitMQ:Username", "guest" },
                    { "RabbitMQ:Password", "guest" }
                });
        }
        else
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RabbitMQ:Host", "localhost" },
                    { "RabbitMQ:Username", "guest" },
                    { "RabbitMQ:Password", "guest" }
                });
        }
        var configuration = configBuilder.Build();

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"],
            UserName = configuration["RabbitMQ:Username"],
            Password = configuration["RabbitMQ:Password"]
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        Console.WriteLine("Listing messages in the dead-letter queue:");

        var result = await channel.BasicGetAsync(DeadLetterQueueName, autoAck: false);
        int counter = 0;
        while (result != null)
        {
            var message = Encoding.UTF8.GetString(result.Body.ToArray());
            Console.WriteLine($"Message: {message}");
            ++counter;
            result = await channel.BasicGetAsync(DeadLetterQueueName, autoAck: false);
        }

        Console.WriteLine($"Finished listing {counter} messages.");
    }

    private static async Task RetryMessages(bool isProd)
    {
        Console.WriteLine($"Using configuration: {(isProd ? "production" : "development")}");
        var configBuilder = new ConfigurationBuilder();
        if (isProd)
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RabbitMQ:Host", "192.172.25.45" },
                    { "RabbitMQ:Username", "guest" },
                    { "RabbitMQ:Password", "guest" }
                });
        }
        else
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "RabbitMQ:Host", "localhost" },
                    { "RabbitMQ:Username", "guest" },
                    { "RabbitMQ:Password", "guest" }
                });
        }
        var configuration = configBuilder.Build();

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"],
            UserName = configuration["RabbitMQ:Username"],
            Password = configuration["RabbitMQ:Password"]
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        var deadLetterMessages = new List<byte[]>();

        // Consume all messages from the dead-letter queue
        var result = await channel.BasicGetAsync(DeadLetterQueueName, autoAck: false);
        while (result != null)
        {
            deadLetterMessages.Add(result.Body.ToArray());
            await channel.BasicAckAsync(result.DeliveryTag, multiple: false);
            result = await channel.BasicGetAsync(DeadLetterQueueName, autoAck: false);
        }

        Console.WriteLine($"Retrieved {deadLetterMessages.Count} messages from the dead-letter queue.");

        // Re-queue messages into the main queue
        foreach (var body in deadLetterMessages)
        {
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Re-queuing message: {message}");
            await channel.BasicPublishAsync(exchange: "", routingKey: MainQueueName, mandatory: true, body: body);
        }

        Console.WriteLine("All messages have been re-queued.");

        Console.WriteLine("Press [enter] to exit.");
        Console.ReadLine();
    }
}
