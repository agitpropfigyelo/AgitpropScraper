using webscraper;

internal class Program
{
    private static void Main(string[] args)
    {
        List<string> sitesToScrape =
        [
            "ripost",
            "mandiner",
            "metropol",
            "origo",
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
        List<IArchiveScraperService> archiveScrapers = ArchiveScraperFactory.GetScraperForSites(sitesToScrape);
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

        //létezik-e a cikk az adatbázisban?

        //feldolgozni az összes cikket
        List<Article> articles = archiveScrapers.SelectMany(scraper => scraper.GetArticlesForDayAsync(datesToScrape[0]).Result).ToList();
        //ner-elés
        foreach (Article article in articles)
        {
            System.Console.WriteLine("-----------------------------");
            try
            {
                System.Console.WriteLine(article.Url);
                IArticleScraperService scraper = ArticleScraperFactory.GetScraperForSite(article.Source);
                article.Corpus=scraper.GetCorpus(article);
                System.Console.WriteLine(article.Corpus);
                
            }
            catch (System.Exception)
            {
                System.Console.WriteLine("Failed");
            }
        }
        //beírni adatbázisba
    }
}