using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Infrastructure.PageLoader;
using Agitprop.Infrastructure.PageRequester;
using Microsoft.Extensions.DependencyInjection;

namespace Agitprop.Infrastructure;

public static class Extensions
{
    public static IServiceCollection ConfigureInfrastructureWithoutBrowser(this IServiceCollection services)
    {
        services.AddTransient<ISpider, Spider>();
        services.AddTransient<ICookiesStorage, CookieStorage>();
        services.AddTransient<IStaticPageLoader, HttpStaticPageLoader>();
        services.AddTransient<IPageRequester, RotatingProxyPageRequester>();
        services.AddTransient<IProxyProvider, ProxyScrapeProxyProvider>();
        return services;
    }
}
