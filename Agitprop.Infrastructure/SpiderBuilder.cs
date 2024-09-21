using System.Net;
using Microsoft.Extensions.Logging;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Infrastructure.PageRequester;
using Agitprop.Core;
using Agitprop.Infrastructure.PageLoader;

namespace Agitprop.Infrastructure;

public class SpiderBuilder
{
    private List<ISink> Sinks = [];
    private ILogger Logger;
    private IScraperConfigStore ScraperConfigStore;
    private ILinkTracker LinkTracker;
    private IStaticPageLoader? StaticPageLoader;
    private IBrowserPageLoader? BrowserPageLoader;
    private IProxyProvider? ProxyProvider;
    private CookieContainer Cookies { get; } = new();
    public ICookiesStorage CookieStorage { get; private set; }

    public Spider Build()
    {
        if (ProxyProvider != null)
        {
            BrowserPageLoader ??= new PuppeteerPageLoaderWithProxies(Logger, ProxyProvider, CookieStorage);

            var pageRequester = new RotatingProxyPageRequester(ProxyProvider);

            StaticPageLoader ??= new HttpStaticPageLoader(pageRequester, CookieStorage, Logger);
        }
        else
        {
            var pageRequester = new PageRequester.PageRequester();

            StaticPageLoader ??= new HttpStaticPageLoader(pageRequester, CookieStorage, Logger);
            BrowserPageLoader ??= new PuppeteerPageLoader(Logger, CookieStorage);
        }

        CookieStorage?.AddAsync(Cookies);

        var spider = new Spider(
            Sinks,
            LinkTracker,
            StaticPageLoader,
            BrowserPageLoader,
            ScraperConfigStore,
            Logger);

        return spider;
    }
    public SpiderBuilder WithSink(ISink sink)
    {
        Sinks.Add(sink);
        return this;
    }

    public SpiderBuilder WithConfigStorage(IScraperConfigStore scraperConfigStorage)
    {
        ScraperConfigStore = scraperConfigStorage;
        return this;
    }

    public SpiderBuilder WithBrowserPageLoader(IBrowserPageLoader browserPageLoader)
    {
        BrowserPageLoader = browserPageLoader;
        return this;
    }

    public SpiderBuilder WithProxies(IProxyProvider proxyProvider)
    {
        ProxyProvider = proxyProvider;
        return this;
    }

    public SpiderBuilder WithCookieStorage(ICookiesStorage cookiesStorage)
    {
        CookieStorage = cookiesStorage;
        return this;
    }
    public SpiderBuilder WithLinkTracker(ILinkTracker linkTracker)
    {
        LinkTracker = linkTracker;
        return this;
    }
    public SpiderBuilder WithLogger(ILogger logger)
    {
        Logger = logger;
        return this;
    }

}