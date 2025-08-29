using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Agitprop.Infrastructure.SurrealDB;

public static class Extensions
{
    public static IHostApplicationBuilder AddNewsfeedDB(this IHostApplicationBuilder builder)
    {
        builder.AddSurrealDbConnection();
        builder.Services.AddTransient<INewsfeedDB, NewsfeedDB>();
        return builder;
    }
    public static IHostApplicationBuilder AddNewsfeedRepositories(this IHostApplicationBuilder builder)
    {
        builder.AddSurrealDbConnection();
        builder.Services.AddTransient<IEntityRepository, EntityRepository>();
        return builder;
    }

    private static IHostApplicationBuilder AddSurrealDbConnection(this IHostApplicationBuilder builder)
    {
        var surrealConnectionString = builder.Configuration.GetConnectionString("surrealdb");
        builder.Services.AddSurreal(options =>
        {
            options.FromConnectionString(surrealConnectionString);
            options.WithNamespace("agitprop");
            options.WithDatabase("newsfeed");
        });
        builder.Services.AddTransient<IEntityRepository, EntityRepository>();
        return builder;
    }
}
