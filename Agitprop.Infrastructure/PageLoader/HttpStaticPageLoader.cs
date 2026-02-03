using Microsoft.Extensions.Configuration;
using Polly;
using System.Net;
using System.Text;
using System.Diagnostics;

using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure.PageLoader;

/// <summary>
/// A static page loader that uses HTTP requests to fetch page content.
/// </summary>
public class HttpStaticPageLoader : IStaticPageLoader
{
    private readonly int _retryCount;
    private readonly ActivitySource ActivitySource = new("Agitprop.PageLoader.HttpStaticPageLoader");

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
        _retryCount = configuration?.GetValue<int>("Retry:PageLoader") ?? 3;
    }

    public IPageRequester PageRequester { get; }
    public ICookiesStorage CookiesStorage { get; }
    public ILogger<HttpStaticPageLoader>? Logger { get; }

    /// <summary>
    /// Loads the content of a static page from the specified URL.
    /// </summary>
    public async Task<string> Load(string url)
    {
        using var trace = ActivitySource.StartActivity("Load", ActivityKind.Producer);
        trace?.SetTag("url", url);

        Logger?.LogInformation("Starting to load page: {url}", url);

        PageRequester.CookieContainer = await CookiesStorage.GetAsync();

        HttpResponseMessage response = null!;
        try
        {
            response = await Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    _retryCount,
                    attempt => TimeSpan.FromSeconds(0.5 * attempt),
                    (outcome, ts, attempt, ctx) =>
                    {
                        if (outcome.Exception != null)
                            Logger?.LogWarning(outcome.Exception, "[RETRY] Exception loading page {url} on attempt {attempt}", url, attempt);
                        else if (outcome.Result != null)
                            Logger?.LogWarning("[RETRY] Failed to load page {url} on attempt {attempt}. Status: {statusCode}", url, attempt, outcome.Result.StatusCode);
                    })
                .ExecuteAsync(() => PageRequester.GetAsync(url));

            if (response.IsSuccessStatusCode)
            {
                Logger?.LogInformation("Successfully loaded page: {url}", url);
                return await response.Content.ReadAsStringAsync();
            }

            Logger?.LogError("Failed to load page {url}. Status code: {statusCode}", url, response.StatusCode);
            trace?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, $"StatusCode={response.StatusCode}");
            throw new InvalidOperationException($"Failed to load page {url}. Status code: {response.StatusCode}. Headers: {response.Headers}")
            {
                Data = { ["url"] = url, ["statusCode"] = response.StatusCode, ["headers"] = response.Headers }
            };
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Exception thrown while loading page {url}", url);
            trace?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
