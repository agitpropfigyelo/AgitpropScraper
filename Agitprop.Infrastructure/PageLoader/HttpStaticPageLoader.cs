using Microsoft.Extensions.Configuration;
using Polly;
using System.Net;
using System.Text;

using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure.PageLoader;

/// <summary>
/// A static page loader that uses HTTP requests to fetch page content.
/// </summary>
public class HttpStaticPageLoader : IStaticPageLoader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpStaticPageLoader"/> class.
    /// </summary>
    /// <param name="pageRequester">The page requester to use for sending HTTP requests.</param>
    /// <param name="cookiesStorage">The storage for managing cookies.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    private readonly int _retryCount;

    public HttpStaticPageLoader(
        IPageRequester pageRequester,
        ICookiesStorage cookiesStorage,
        ILogger<HttpStaticPageLoader>? logger = default,
        IConfiguration? configuration = null)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        PageRequester = pageRequester;
        CookiesStorage = cookiesStorage;
        Logger = logger;
    _retryCount = configuration?.GetValue<int>("Retry:PageLoader", 3) ?? 3;
    }

    /// <summary>
    /// Gets the page requester used for sending HTTP requests.
    /// </summary>
    public IPageRequester PageRequester { get; }

    /// <summary>
    /// Gets the storage for managing cookies.
    /// </summary>
    public ICookiesStorage CookiesStorage { get; }

    /// <summary>
    /// Gets the logger for logging information and errors.
    /// </summary>
    public ILogger<HttpStaticPageLoader>? Logger { get; }

    /// <summary>
    /// Loads the content of a static page from the specified URL.
    /// </summary>
    /// <param name="url">The URL of the page to load.</param>
    /// <returns>The content of the page as a string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the page cannot be loaded successfully.</exception>
    public async Task<string> Load(string url)
    {
        PageRequester.CookieContainer = await CookiesStorage.GetAsync(); // TODO move to init factory func

        HttpResponseMessage response = null!;
        try
        {
            response = await Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt), (outcome, ts, attempt, ctx) =>
                {
                    if (outcome.Exception != null)
                        Logger?.LogWarning(outcome.Exception, "[RETRY] Exception loading page {url} on attempt {attempt}", url, attempt);
                    else if (outcome.Result != null)
                        Logger?.LogWarning("[RETRY] Failed to load page {url} on attempt {attempt}. Status: {statusCode}", url, attempt, outcome.Result.StatusCode);
                })
                .ExecuteAsync(() => PageRequester.GetAsync(url));
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Exception thrown while loading page {url}", url);
            throw;
        }

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        Logger?.LogError("Failed to load page {url}. Error code: {statusCode}", url, response.StatusCode);
        throw new InvalidOperationException($"Failed to load page {url}. Error code: {response.StatusCode}. Headers: {response.Headers}")
        {
            Data = { ["url"] = url, ["statusCode"] = response.StatusCode, ["headers"] = response.Headers }
        };
    }
}
