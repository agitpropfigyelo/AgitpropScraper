using Agitprop.Scraper.Sinks.Newsfeed.Database;
using Agitprop.Scraper.Sinks.Newsfeed.Factories;
using Agitprop.Scraper.Sinks.Newsfeed.Interfaces;

using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Exceptions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agitprop.Scraper.Sinks.Newsfeed;

public static class Extensions
{
    public static IServiceCollection AddNewsfeedSink(this IServiceCollection services, IConfiguration configuration)
    {
        var newsfeedConfig = configuration.GetSection("NewsfeedSink") ?? throw new MissingConfigurationValueException("Missing config section for NewsfeedSink");
        var surrealDbConnectionString = newsfeedConfig.GetValue<string>("SurrealDB");
        if (string.IsNullOrEmpty(surrealDbConnectionString))
        {
            throw new MissingConfigurationValueException("Missing config for SurrealDB");
        }

        var nerBaseUrl = newsfeedConfig["NERbaseUrl"];
        if (string.IsNullOrEmpty(nerBaseUrl))
        {
            throw new MissingConfigurationValueException("Missing config for NERbaseUrl");
        }

        services.AddTransient<INamedEntityRecognizer, NamedEntityRecognizer>();

        services.AddHttpClient<INamedEntityRecognizer, NamedEntityRecognizer>(client =>
        {
            client.BaseAddress = new Uri(nerBaseUrl);
        });

        services.AddTransient<INewsfeedDB, NewsfeedDB>();
        services.AddTransient<NewsfeedSink>();
        services.AddSurreal(surrealDbConnectionString);

        return services;
    }


    public static ScrapingJob ConvertToScrapingJob(this NewsfeedJobDescrpition description)
    {
        var site = GetNewssiteFromUrl(description.Url);
        return description switch
        {
            { Type: PageContentType.Article } => ScrapingJobFactory.GetArticleScrapingJob(site, description.Url),
            { Type: PageContentType.Archive } => ScrapingJobFactory.GetArchiveScrapingJob(site, description.Url),
            _ => throw new ArgumentException($"Not supported newsfeed job type: {description.Type}")
        };
    }

    private static NewsSites GetNewssiteFromUrl(string url)
    {
        var uri = new Uri(url);
        return uri.Host.ToLower() switch
        {
            "www.origo.hu" => NewsSites.Origo,
            "origo.hu" => NewsSites.Origo,
            "www.ripost.hu" => NewsSites.Ripost,
            "ripost.hu" => NewsSites.Ripost,
            "www.mandiner.hu" => NewsSites.Mandiner,
            "mandiner.hu" => NewsSites.Mandiner,
            "www.metropol.hu" => NewsSites.Metropol,
            "metropol.hu" => NewsSites.Metropol,
            "www.magyarnemzet.hu" => NewsSites.MagyarNemzet,
            "magyarnemzet.hu" => NewsSites.MagyarNemzet,
            "www.pestisracok.hu" => NewsSites.PestiSracok,
            "pestisracok.hu" => NewsSites.PestiSracok,
            "www.magyarjelen.hu" => NewsSites.MagyarJelen,
            "magyarjelen.hu" => NewsSites.MagyarJelen,
            "www.kuruc.info" => NewsSites.Kurucinfo,
            "kuruc.info" => NewsSites.Kurucinfo,
            "www.alfahir.hu" => NewsSites.Alfahir,
            "alfahir.hu" => NewsSites.Alfahir,
            "www.24.hu" => NewsSites.Huszonnegy,
            "24.hu" => NewsSites.Huszonnegy,
            "www.444.hu" => NewsSites.NegyNegyNegy,
            "444.hu" => NewsSites.NegyNegyNegy,
            "www.hvg.hu" => NewsSites.HVG,
            "hvg.hu" => NewsSites.HVG,
            "www.telex.hu" => NewsSites.Telex,
            "telex.hu" => NewsSites.Telex,
            "www.rtl.hu" => NewsSites.RTL,
            "rtl.hu" => NewsSites.RTL,
            "www.index.hu" => NewsSites.Index,
            "index.hu" => NewsSites.Index,
            "www.merce.hu" => NewsSites.Merce,
            "merce.hu" => NewsSites.Merce,
            _ => throw new ArgumentException($"Not supported news source: {uri.Host}", nameof(uri))
        };
    }
}
