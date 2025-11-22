using System.Net;

using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.PageLoader;
using Agitprop.Infrastructure.PageRequester;
using Agitprop.Infrastructure.ProxyProviders;

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

        builder.Services.AddHttpClient<ProxyScrapeProxyProvider>();
        builder.Services.AddSingleton<IProxyProvider, ProxyScrapeProxyProvider>();

        builder.Services.AddHttpClient<RedScrapeProxyProvider>();
        builder.Services.AddSingleton<IProxyProvider, RedScrapeProxyProvider>();

        builder.Services.AddSingleton<IProxyPool, ProxyPoolService>();
        builder.Services.AddSingleton<RotatingHttpClientPool>();
        builder.Services.AddTransient<IPageRequester, RotatingProxyPageRequester>();
        builder.Services.AddHostedService<ProxyInitializationService>();


        //TODO: Puppeteer not working w/ proxies
        builder.Services.AddTransient<IBrowserPageLoader, PuppeteerPageLoader>();

        return builder;
    }
}
