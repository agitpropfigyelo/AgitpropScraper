using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using webscraper;

internal class Program
{
    private static async Task Main(string[] args)
    {
        List<string> sitesToScrape =
        [
            "origo",
            "ripost",
            "mandiner",
            "metropol",
            "magyarnemzet",
            "pestisracok",
            "magyarjelen",
            "kuruczinfo",
            "alfahir",
            "24hu",
            "444",
            "telex",
            "rtl",
            "index",
            "merce"
        ];
        Console.WriteLine("Start");
        //lekérni az összes cikket
        List<DateTime> datesToScrape = [
            new DateTime(2024, 02, 10),
            // new DateTime(2001, 09, 11),
            // new DateTime(2015, 11, 14),
            // new DateTime(2015, 02, 06),
            // new DateTime(2015, 02, 07),
            // new DateTime(2022, 02, 13),
            // new DateTime(2022, 02, 14),
            // new DateTime(2022, 02, 23),
        ];
        var tryDate = new DateTime(2024, 02, 10);

        List<IArchiveScraperService> archiveScrapers = ArchiveScraperFactory.GetScraperForSites(sitesToScrape);
        INerService nerService = new LocalNerService();
        SurrealDBService dbService = new();

        //idk ez mennyire aszinkron?
        IEnumerable<List<Article>> articlesBySource = archiveScrapers.Select(async x => await x.GetArticlesForDayAsync(tryDate)).Select(x => x.Result.ToList());
        
    }
}