using System.CommandLine;
using System.Text;
using System.Text.Json;

using Agitprop.Core.Enums;
using Agitprop.Core.Factories;

using RabbitMQ.Client;

class Program
{
    private static readonly string QueueName = "scraping-job";

    public static async Task Main(string[] args)
    {
        // Define command-line options
        var rootCommand = new RootCommand();

        var dateOption = new Option<DateOnly>(
            new[] { "--date", "-d" },
            () => DateOnly.FromDateTime(DateTime.Today - TimeSpan.FromDays(1)),
            "Specifies the date for the scraping job descriptions");
        var siteOption = new Option<NewsSites[]>(
            new[] { "--site", "-s" },
            () => Enum.GetValues<NewsSites>(),
            "Specifies the news sites to include (comma-separated or multiple flags)");


        rootCommand.Description = "Scraping Job Publisher - Publishes scraping jobs to RabbitMQ";

        var addCommand = new Command("add", "Publishes scraping jobs to queue")
        {
            dateOption,
            siteOption,
        };
        rootCommand.AddCommand(addCommand);



        addCommand.SetHandler(async (date, sites) => await PublishJob(date, sites), dateOption, siteOption);


        await rootCommand.InvokeAsync(args);
    }



    internal static async Task PublishJob(DateOnly date, NewsSites[] sites)
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

        await Task.CompletedTask; // Simulates async behavior for consistency
    }
}
