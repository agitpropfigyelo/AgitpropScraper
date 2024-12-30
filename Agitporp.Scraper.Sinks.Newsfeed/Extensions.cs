using System;
using Agitporp.Scraper.Sinks.Newsfeed.Factories;
using Agitporp.Scraper.Sinks.Newsfeed.Interfaces;
using Agitprop.Core;
using Agitprop.Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Agitporp.Scraper.Sinks.Newsfeed;

public static class Extensions
{
    public static IServiceCollection AddNewsfeedSink(this IServiceCollection services)
    {
        //TODO: properly
        services.AddSingleton<INamedEntityRecognizer, NamedEntityRecognizer>();
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
