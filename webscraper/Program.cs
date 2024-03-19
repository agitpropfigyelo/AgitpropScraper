using System.Collections.Concurrent;
using webscraper;

internal class Program
{
    private static async Task Main(string[] args)
    {
        List<string> sitesToScrape =
        [
            "origo",
            //"ripost",
            //"mandiner",
            //"metropol",
            //"magyarnemzet",
            //"pestisracok",
            //"magyarjelen",
            //"kuruczinfo",
            //"alfahir",
            //"24hu",
            //"444",
            //"telex",
            //"rtl",
            //"index",
            //"merce"
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

        foreach (var articlesFromOneSource in articlesBySource)
        {
            string articleSource = articlesFromOneSource[0].Source;
            IArticleScraperService articleScraper = ArticleScraperFactory.GetScraperForSite(articleSource);
            List<Article> successfullScrape = await articleScraper.GetCorpus(articlesFromOneSource, new FailedReportHandler()); //itt már csak a scrapelt oldalak vannak
            System.Console.WriteLine($"{articlesFromOneSource.Count} / {successfullScrape.Count}");
            List<Article> successfullNer = await nerService.AnalyzeBatch(successfullScrape);
            var tasks = successfullNer.Select(x => dbService.CreateMentionsForArticle(x)).ToArray();
            Task.WaitAll(tasks);
            foreach (var item in successfullNer)
            {
                await dbService.CreateMentionsForArticle(item);
                System.Console.WriteLine("-----------------");
                System.Console.WriteLine(item);
            }
        }



        //     //OLD BUT GOLD
        //     IEnumerable<Article> articles = archiveScrapers.SelectMany(scraper => scraper.GetArticlesForDayAsync(datesToScrape[0]).Result);

        //     ConcurrentBag<Article> sendToNer = [];
        //     ConcurrentBag<(Article, Exception)> failedScraping = [];

        //     System.Console.WriteLine($"Getting articles from {datesToScrape[0]}");

        //     articles.AsParallel().ForAll(async article =>
        //     {
        //         try
        //         {
        //             article.Corpus = await ArticleScraperFactory.GetScraperForSite(article.Source).GetCorpus(article);
        //             sendToNer.Add(article);
        //         }
        //         catch (System.Exception ex)
        //         {
        //             failedScraping.Add((article, ex));
        //         }
        //     });

        //     System.Console.WriteLine($"{articles.Count()} / {sendToNer.Count}");


        //     IEnumerable<Article[]> chunks = sendToNer.Chunk(5);
        //     ConcurrentBag<(Article, Exception)> failedNer = [];
        //     ConcurrentBag<Article> successfulNer = [];

        //     chunks.AsParallel().ForAll(chunk =>
        //     {
        //         try
        //         {

        //             List<NerResponse> responseList = nerService.AnalyzeBatch([.. chunk]).Result;
        //             foreach ((Article article, NerResponse response) in chunk.Zip(responseList))
        //             {
        //                 article.Entities = response;
        //                 successfulNer.Add(article);
        //             }
        //         }
        //         catch (System.Exception ex)
        //         {
        //             foreach (var item in chunk)
        //             {
        //                 failedNer.Add((item, ex));
        //             }
        //         }
        //     });
        //     System.Console.WriteLine($"Successfully got entities: {successfulNer.Count}");

        //     SurrealDBService dbService = new();

        //     foreach (var item in successfulNer)
        //     {
        //         dbService.CreateMentionsForArticle(item);
        //     }


        // }
        //beírni adatbázisba

        //System.Console.WriteLine($"{articles.Count} / {articles.Count-failedCount}");
    }

    class FailedReportHandler : IProgress<Article>
    {
        public void Report(Article value)
        {
            System.Console.WriteLine($"Failed to scrape {value.Url}");
        }
    }
}