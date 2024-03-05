namespace webscraper;

public interface IArchiveScraperService
{
        public Task<IEnumerable<Article>> GetArticlesForDayAsync(DateTime dateIn);

}
