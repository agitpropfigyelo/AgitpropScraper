using webscraper;

internal class Program
{
    private static void Main(string[] args)
    {
        //lekérni az összes cikket
        List<DateTime> datesToScrape = [
                    // new DateTime(2024, 02, 10),
                    // new DateTime(2001, 09, 11),
                    // new DateTime(2015, 11, 14),
                    // new DateTime(2015, 02, 06),
                    new DateTime(2015, 02, 07),
            // new DateTime(2022, 02, 13),
            // new DateTime(2022, 02, 14),
            // new DateTime(2022, 02, 23),
        ];
        List<IArchiveScraperService> archiveScrapers =
        [
            //TODO:
            //new SitemapScraperArchive("https://ripost.hu/","ripost"),
                //kormany
            //new SitemapScraperArchive("https://mandiner.hu/","mandiner"),
            //new SitemapScraperArchive("https://metropol.hu/","metropol"),
            new OrigoArchiveScraper(),
            //magyarnemzet
            //pestisracok
                //szelsojobb
            //magyarjelen
            //kuruczinfo
            //alfahir
                //libsi
            //24hu
            //444
            //hvg
            //telex
            //rtl
            //index
                //bal
            //merce
        ];
        //létezik-e a cikk az adatbázisban?

        //feldolgozni az összes cikket

        //ner-elés

        //beírni adatbázisba
        Console.WriteLine("Hello, World!");
    }
}