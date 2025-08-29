using System.Net;
using System.Text;

using Agitprop.Infrastructure.Interfaces;

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
    public HttpStaticPageLoader(IPageRequester pageRequester, ICookiesStorage cookiesStorage, ILogger<HttpStaticPageLoader>? logger = default)
    {
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        PageRequester = pageRequester;
        CookiesStorage = cookiesStorage;
        Logger = logger;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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

        var response = await PageRequester.GetAsync(url);

        if (response.IsSuccessStatusCode) return await response.Content.ReadAsStringAsync();

        Logger?.LogError("Failed to load page {url}. Error code: {statusCode}", url, response.StatusCode);

        throw new InvalidOperationException($"Failed to load page {url}. Error code: {response.StatusCode}. Headers: {response.Headers}")
        {
            Data = { ["url"] = url, ["statusCode"] = response.StatusCode, ["headers"] = response.Headers }
        };
    }
}
