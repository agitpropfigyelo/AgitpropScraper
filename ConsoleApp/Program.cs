using NewsArticleScraper.Core;
using NewsArticleScraper.Scrapers;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        DateTime date = new(2024, 03, 11);

        var origo = new OrigoScraper();

        var idk = await origo.GetArticlesForDateAsync(date);

        List<ArticleInfo> allArticles=[];

        foreach (var item in idk)
        {
            System.Console.WriteLine("--------------");
            try
            {
                //NewsArticleScraper.Core.ArticleInfo asd = origo.GetArticleInfo(item, date);
                //allArticles.Add(asd);
            }
            catch (System.Exception)
            {
                System.Console.WriteLine($"Failed: {item}");

            }
        }
    }
}