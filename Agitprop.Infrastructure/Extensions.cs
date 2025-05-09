using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Infrastructure.PageLoader;
using Agitprop.Infrastructure.PageRequester;

using Microsoft.Extensions.DependencyInjection;

namespace Agitprop.Infrastructure;

/// <summary>
/// Provides extension methods for configuring infrastructure services.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Configures infrastructure services without a browser, with optional proxy support.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="useProxies">Indicates whether to use proxies for HTTP requests.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection ConfigureInfrastructureWithoutBrowser(this IServiceCollection services, bool useProxies = false)
    {
        services.AddTransient<ISpider, Spider>();
        services.AddTransient<ICookiesStorage, CookieStorage>();
        services.AddTransient<IStaticPageLoader, HttpStaticPageLoader>();
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
