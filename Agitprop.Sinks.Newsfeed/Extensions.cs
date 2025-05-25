using Agitprop.Scraper.Sinks.Newsfeed.Database;
using Agitprop.Scraper.Sinks.Newsfeed.Factories;
using Agitprop.Scraper.Sinks.Newsfeed.Interfaces;

using Agitprop.Core;
using Agitprop.Core.Enums;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Agitprop.Scraper.Sinks.Newsfeed;

/// <summary>
/// Provides extension methods for configuring and converting newsfeed-related services and jobs.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds the Newsfeed Sink services to the application builder.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The updated host application builder.</returns>
    public static IHostApplicationBuilder AddNewsfeedSink(this IHostApplicationBuilder builder)
    {
        var surrealConnectionString = builder.Configuration.GetConnectionString("surrealdb");
        builder.Services.AddSurreal(options=>
        {
            options.FromConnectionString(surrealConnectionString);
            options.WithNamespace("agitprop");
            options.WithDatabase("newsfeed");
        });

        builder.Services.AddTransient<INamedEntityRecognizer, NamedEntityRecognizer>();

        var nlp = builder.Configuration.GetValue<string>("nlpService");
        var idk = builder.Services.AddHttpClient<INamedEntityRecognizer, NamedEntityRecognizer>(client =>
        {
            client.BaseAddress = new Uri(nlp);
        });
        builder.Services.AddTransient<INewsfeedDB, NewsfeedDB>();
        builder.Services.AddTransient<NewsfeedSink>();
        return builder;
    }

    /// <summary>
    /// Converts a <see cref="NewsfeedJobDescrpition"/> to a <see cref="ScrapingJob"/>.
    /// </summary>
    /// <param name="description">The newsfeed job description to convert.</param>
    /// <returns>A <see cref="ScrapingJob"/> instance.</returns>
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

    /// <summary>
    /// Determines the news site from a given URL.
    /// </summary>
    /// <param name="url">The URL to analyze.</param>
    /// <returns>The corresponding <see cref="NewsSites"/> enum value.</returns>
    /// <exception cref="ArgumentException">Thrown if the news source is not supported.</exception>
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
            "www.24.hu" => NewsSites.HuszonnegyHu,
            "24.hu" => NewsSites.HuszonnegyHu,
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
