using System.Net;

using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Infrastructure.PageLoader;
using Agitprop.Infrastructure.PageRequester;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Agitprop.Infrastructure.Puppeteer;

/// <summary>
/// Provides extension methods for configuring Puppeteer-based infrastructure services.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Configures infrastructure services with Puppeteer browser support, with optional proxy support.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="useProxies">Indicates whether to use proxies for HTTP requests.</param>
    /// <returns>The updated host application builder.</returns>
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
