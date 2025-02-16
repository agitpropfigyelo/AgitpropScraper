using System.CommandLine;
using System.Text;
using System.Text.Json;

using Agitprop.Core.Enums;
using Agitprop.Scraper.Sinks.Newsfeed;

using RabbitMQ.Client;

namespace Agitprop.ConsoleToolKit;

public static class QueueCommand
{
    private static readonly string QueueName = "newsfeed-job";
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
        var articleOption = new Option<string>(
            ["--article"],
            "Add an article page to queue");
        var archiveOption = new Option<string>(
            ["--archive"],
            "Add an archive page to queue");

        var addCommand = new Command(CommandName, "Publishes scraping jobs to queue")
        {
            dateOption,
            siteOption,
            articleOption,
            archiveOption
        };
        addCommand.SetHandler(PublishJob, dateOption, siteOption, articleOption, archiveOption);

        rootCommand.Add(addCommand);
        return addCommand;
    }

    private static async Task PublishJob(DateOnly date, NewsSites[] sites, string? article, string? archive)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        Console.WriteLine($"Running with date: {date}, sites: {sites.Length}, article: {article}, archive: {archive}");

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: QueueName,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

        if (!string.IsNullOrEmpty(article))
        {
            var message = JsonSerializer.Serialize(new NewsfeedJobDescrpition { Url = article, Type = PageContentType.Article });
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchange: "",
                                            routingKey: QueueName,
                                            mandatory: true,
                                            body: body);

            Console.WriteLine($"Published article with URL: {message}");
            return;
        }
        if (!string.IsNullOrEmpty(archive))
        {
            var message = JsonSerializer.Serialize(new NewsfeedJobDescrpition { Url = archive, Type = PageContentType.Archive });
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchange: "",
                                            routingKey: QueueName,
                                            mandatory: true,
                                            body: body);

            Console.WriteLine($"Published archive with URL: {message}");
            return;
        }
        foreach (var site in sites)
        {
            try
            {
                //create newsfeedJob by date
                var message = JsonSerializer.Serialize(CreateJob(date, site));
                var body = Encoding.UTF8.GetBytes(message);

                await channel.BasicPublishAsync(exchange: "",
                                                routingKey: QueueName,
                                                mandatory: true,
                                                body: body);

                Console.WriteLine($"Published job: {message}");
            }
            catch (Exception)
            {
                Console.WriteLine($"Failed to queue: {site}");
            }
        }

        await Task.CompletedTask;
    }

    private static NewsfeedJobDescrpition CreateJob(DateOnly date, NewsSites site)
    {
        string url = site switch
        {
            NewsSites.Origo => $"https://www.origo.hu/hirarchivum/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}",
            NewsSites.Ripost => $"https://ripost.hu/{date:yyyyMM}_sitemap.xml",
            NewsSites.Mandiner => $"https://mandiner.hu/{date:yyyyMM}_sitemap.xml",
            NewsSites.Metropol => $"https://metropol.hu/{date:yyyyMM}_sitemap.xml",
            NewsSites.MagyarNemzet => $"https://magyarnemzet.hu/{date:yyyyMM}_sitemap.xml",
            NewsSites.PestiSracok => $"https://www.pestisracok.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}",
            NewsSites.MagyarJelen => $"https://www.magyarjelen.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}",
            NewsSites.Kurucinfo => throw new NotImplementedException("Kurucinfo scraping is not yet supported"),
            NewsSites.Alfahir => throw new NotImplementedException("Scraping by date is not supported"),//"https://alfahir.hu/hirek/oldalak/1",
            NewsSites.Huszonnegy => $"https://www.24.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}",
            NewsSites.NegyNegyNegy => $"https://www.444.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}",
            NewsSites.HVG => $"https://www.hvg.hu/frisshirek/{date.Year:D4}.{date.Month:D2}.{date.Day:D2}",
            NewsSites.Telex => $"https://telex.hu/sitemap/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}/news.xml",
            NewsSites.RTL => throw new NotImplementedException("Scraping by date is not supported"),//$"https://rtl.hu/legfrissebb?oldal=1",
            NewsSites.Index => $"https://index.hu/sitemap/cikkek_{date:yyyyMM}.xml",
            NewsSites.Merce => $"https://www.merce.hu/{date.Year:D4}/{date.Month:D2}/{date.Day:D2}",
            _ => throw new NotImplementedException(),
        }
        ;
        return new NewsfeedJobDescrpition
        {
            Url = url,
            Type = PageContentType.Archive
        };
    }
}
