

using System.Diagnostics;
using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace webscraper;

public class Article
{
    public Uri Url { get; init; }
    public string Source { get; init; }
    public DateTime Date { get; init; }
    public string? Corpus { get; set; }
    [JsonIgnore] private List<Func<HtmlDocument, string>>? ScraperFunctions { get; init; }
    [JsonIgnore] private HtmlDocument? Doc { get; set; }

    public Article(string urlIn, DateTime dateIn, string sourceIn)
    {
        Url = new Uri(urlIn);
        Date = dateIn;
        Source = sourceIn;
    }
    
    public Article(Uri urlIn, DateTime dateIn, string sourceIn)
    {
        Url = urlIn;
        Date = dateIn;
        Source = sourceIn;
    }
    public Task<Article> GetHtmlAsync(IProgress<(TimeSpan, string)>? progress, CancellationToken? token = null)
    {
        HtmlWeb web = new HtmlWeb();
        web.OverrideEncoding = Encoding.UTF8;
        TaskCompletionSource<Article> tcs = new TaskCompletionSource<Article>();
        Stopwatch stopwatch = new();
        try
        {
            stopwatch.Start();
            this.Doc = web.Load(Url);
            tcs.SetResult(this);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            tcs.SetException(ex);
            progress?.Report((stopwatch.Elapsed, "Failed fetching"));
        }
        finally
        {
            stopwatch.Stop();
            progress?.Report((stopwatch.Elapsed, "Fetched"));
        }

        return tcs.Task;
    }

    public async Task<Article> GetHtml(IProgress<(TimeSpan, string)>? progress, CancellationToken? token = null)
    {
        HtmlWeb web = new HtmlWeb();
        web.OverrideEncoding = Encoding.UTF8;
        Stopwatch stopwatch = new();
        try
        {
            stopwatch.Start();
            this.Doc = await web.LoadFromWebAsync(Url.ToString());
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            progress?.Report((stopwatch.Elapsed, $"Failed fetching {ex.Message}"));
        }
        finally
        {
            stopwatch.Stop();
            progress?.Report((stopwatch.Elapsed, "Fetched"));
        }

        return this;
    }

    public Task<Article> GetCorpusAsync(IProgress<(TimeSpan, string)>? progress)
    {
        Stopwatch stopwatch = new();

        TaskCompletionSource<Article> tcs = new TaskCompletionSource<Article>();
        if (Doc is null)
        {
            tcs.SetException(new NoScraperFunctionException($"FAIL {Url}"));
        }
        stopwatch.Start();

        foreach (Func<HtmlDocument, string> scraper in ScraperFunctions!)
        {
            string result = scraper(Doc!);
            if (result.Any())
            {
                stopwatch.Stop();
                Corpus = result;
                progress?.Report((stopwatch.Elapsed, "Scraped"));
                tcs.SetResult(this);
            }
        }
        if (Corpus is null)
        {
            stopwatch.Stop();
            progress?.Report((stopwatch.Elapsed, "Failed scraping"));
            tcs.SetException(new NoScraperFunctionException($"No scraper function available {Url}"));
        }
        return tcs.Task;
    }
    public Article GetCorpus(IProgress<(TimeSpan, string)>? progress)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        foreach (Func<HtmlDocument, string> scraper in ScraperFunctions!)
        {
            string result = scraper(Doc!);
            if (result.Any())
            {
                stopwatch.Stop();
                Corpus = result;
                progress?.Report((stopwatch.Elapsed, "Scraped"));
                return this;
            }
        }
        stopwatch.Stop();
        progress?.Report((stopwatch.Elapsed, "Failed scraping"));
        throw new EmptyCorpusException($"Cannot get corpus of news {Url}");
    }

    public string SerializeJSON()
    {
        return JsonConvert.SerializeObject(this);
    }

    public override string ToString()
    {
        return $"{Url.ToString()}\n{(Corpus is not null ? Corpus![..50] : "Ora et labora")}";
    }
}

