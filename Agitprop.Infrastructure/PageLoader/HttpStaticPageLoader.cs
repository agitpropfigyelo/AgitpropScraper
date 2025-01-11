using System.Net;
using System.Text;

using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;

using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure.PageLoader;

public class HttpStaticPageLoader : IStaticPageLoader
{
    public HttpStaticPageLoader(IPageRequester pageRequester, ICookiesStorage cookiesStorage, ILogger<HttpStaticPageLoader> logger)
    {
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        PageRequester = pageRequester;
        CookiesStorage = cookiesStorage;
        Logger = logger;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    }

    public IPageRequester PageRequester { get; }
    public ICookiesStorage CookiesStorage { get; }
    public ILogger<HttpStaticPageLoader> Logger { get; }

    public async Task<string> Load(string url)
    {
        PageRequester.CookieContainer = await CookiesStorage.GetAsync(); // TODO move to init factory func

        var response = await PageRequester.GetAsync(url);

        if (response.IsSuccessStatusCode) return await response.Content.ReadAsStringAsync();

        Logger.LogError("Failed to load page {url}. Error code: {statusCode}", url, response.StatusCode);

        throw new InvalidOperationException($"Failed to load page {url}. Error code: {response.StatusCode}. Headers: {response.Headers.ToString()}")
        {
            Data = { ["url"] = url, ["statusCode"] = response.StatusCode, ["headers"] = response.Headers }
        };
    }
}
