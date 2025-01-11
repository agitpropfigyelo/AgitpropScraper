using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Infrastructure.PageLoader;
using Agitprop.Infrastructure.PageRequester;

using Microsoft.Extensions.DependencyInjection;

namespace Agitprop.Infrastructure;

public static class Extensions
{
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
