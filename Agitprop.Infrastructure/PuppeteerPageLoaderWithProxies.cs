using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

internal class PuppeteerPageLoaderWithProxies : IBrowserPageLoader
{
    public PuppeteerPageLoaderWithProxies(ILogger logger, IProxyProvider proxyProvider, ICookiesStorage cookieStorage)
    {
        Logger = logger;
        ProxyProvider = proxyProvider;
        CookieStorage = cookieStorage;
    }

    public ILogger Logger { get; }
    public IProxyProvider ProxyProvider { get; }
    public ICookiesStorage CookieStorage { get; }

    public Task<string> Load(string url, object pageActions, bool headless)
    {
        throw new NotImplementedException();
    }
}