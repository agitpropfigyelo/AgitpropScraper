using System.Net;

using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Infrastructure.PageLoader;
using Agitprop.Infrastructure.PageRequester;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Agitprop.Infrastructure.Puppeteer;

public static class Extensions
{
    public static IHostApplicationBuilder ConfigureInfrastructureWithBrowser(this IHostApplicationBuilder builder, bool useProxies = false)
    {
        builder.Services.AddTransient<ISpider, Spider>();
        builder.Services.AddTransient<ICookiesStorage, CookieStorage>();
        builder.Services.AddTransient<IStaticPageLoader, HttpStaticPageLoader>();
        builder.Services.AddTransient<CookieContainer>();
        //TODO: Puppeteer not working w/ proxies
        builder.Services.AddTransient<IBrowserPageLoader, PuppeteerPageLoader>();
        if (useProxies)
        {
            builder.Services.AddTransient<IPageRequester, RotatingProxyPageRequester>();
            builder.Services.AddTransient<IProxyProvider, ProxyScrapeProxyProvider>();
        }
        else
        {
            builder.Services.AddTransient<IPageRequester, PageRequester.PageRequester>();
        }
        return builder;
    }
}
