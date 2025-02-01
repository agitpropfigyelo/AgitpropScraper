using System.Net;

using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Infrastructure.PageLoader;
using Agitprop.Infrastructure.PageRequester;

using Microsoft.Extensions.DependencyInjection;

namespace Agitprop.Infrastructure.Puppeteer;

public static class Extensions
{
    public static IServiceCollection ConfigureInfrastructureWithBrowser(this IServiceCollection services, bool useProxies = false)
    {
        services.AddTransient<ISpider, Spider>();
        services.AddTransient<ICookiesStorage, CookieStorage>();
        services.AddTransient<IStaticPageLoader, HttpStaticPageLoader>();
        services.AddTransient<CookieContainer>();
        //TODO: Puppeteer not working w/ proxies
        services.AddTransient<IBrowserPageLoader, PuppeteerPageLoader>();
        if (useProxies)
        {
            services.AddTransient<IPageRequester, RotatingProxyPageRequester>();
            services.AddTransient<IProxyProvider, ProxyScrapeProxyProvider>();
        }
        else
        {
            services.AddTransient<IPageRequester, PageRequester.PageRequester>();
        }
        return services;
    }
}
