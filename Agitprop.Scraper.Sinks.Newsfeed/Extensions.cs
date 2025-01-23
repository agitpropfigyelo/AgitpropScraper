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
            "ripost.hu" => NewsSites.Ripost,
            "mandiner.hu" => NewsSites.Mandiner,
            "metropol.hu" => NewsSites.Metropol,
            "magyarnemzet.hu" => NewsSites.MagyarNemzet,
            "pestisracok.hu" => NewsSites.PestiSracok,
            "magyarjelen.hu" => NewsSites.MagyarJelen,
            "kuruc.info" => NewsSites.Kurucinfo,
            "alfahir.hu" => NewsSites.Alfahir,
            "24.hu" => NewsSites.Huszonnegy,
            "444.hu" => NewsSites.NegyNegyNegy,
            "hvg.hu" => NewsSites.HVG,
            "telex.hu" => NewsSites.Telex,
            "rtl.hu" => NewsSites.RTL,
            "index.hu" => NewsSites.Index,
            "merce.hu" => NewsSites.Merce,
            _ => throw new ArgumentException($"Not supported news source: {uri.Host}", nameof(uri))
        };
    }
}
