using Agitprop.Core.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Agitprop.Infrastructure.Postgres;

public static class Extensions
{
    public static IHostApplicationBuilder AddNewsfeedDB(this IHostApplicationBuilder builder)
    {
        builder.AddPostgresConnection();
        builder.Services.AddTransient<INewsfeedDB, NewsfeedDB>();
        return builder;
    }
    public static IHostApplicationBuilder AddNewsfeedRepositories(this IHostApplicationBuilder builder)
    {
        builder.AddPostgresConnection();
        builder.Services.AddTransient<IEntityRepository, EntityRepository>();
        builder.Services.AddTransient<ITrendingRepository, TrendingRepository>();
        return builder;
    }

    private static IHostApplicationBuilder AddPostgresConnection(this IHostApplicationBuilder builder)
    {
        var conn = builder.Configuration.GetConnectionString("postgres"); // Aspire provides this if configured
        builder.Services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(conn, o => o.EnableRetryOnFailure()));
        builder.Services.AddTransient<IEntityRepository, EntityRepository>();
        return builder;
    }
}
