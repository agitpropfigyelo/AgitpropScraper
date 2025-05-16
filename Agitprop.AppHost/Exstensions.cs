using System;

using Microsoft.Extensions.DependencyInjection;

namespace Agitprop.AppHost;

public static class Exstensions
{
    public static IResourceBuilder<SurrealDBResource> AddSurrealDB(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        IResourceBuilder<ParameterResource>? userName = null,
        IResourceBuilder<ParameterResource>? password = null,
        int? port = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(name);

        // don't use special characters in the password, since it goes into a URI
        var passwordParameter = password?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password", special: false);

        var surrealDb = new SurrealDBResource(name, userName?.Resource, passwordParameter);

        string? connectionString = null;

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(surrealDb, async (@event, ct) =>
        {
            connectionString = await surrealDb.ConnectionStringExpression.GetValueAsync(ct).ConfigureAwait(false);

            if (connectionString == null)
            {
                throw new DistributedApplicationException($"ConnectionStringAvailableEvent was published for the '{surrealDb.Name}' resource but the connection string was null.");
            }
        });
        //TODO: helathchecks
        // var healthCheckKey = $"{name}_check";
        // // cache the connection so it is reused on subsequent calls to the health check
        // IConnection? connection = null;
        // builder.Services.AddHealthChecks().AddRabbitMQ(async (sp) =>
        // {
        //     // NOTE: Ensure that execution of this setup callback is deferred until after
        //     //       the container is built & started.
        //     return connection ??= await CreateConnection(connectionString!).ConfigureAwait(false);

        //     static Task<IConnection> CreateConnection(string connectionString)
        //     {
        //         var factory = new ConnectionFactory
        //         {
        //             Uri = new Uri(connectionString)
        //         };
        //         return factory.CreateConnectionAsync();
        //     }
        // }, healthCheckKey);

        var rabbitmq = builder.AddResource(surrealDb)
                              .WithImage(SurrealDBContainerImageTags.Image, SurrealDBContainerImageTags.LatestTag)
                              .WithArgs("start")
                              .WithHttpEndpoint(port: port, targetPort: 8000, name: SurrealDBResource.PrimaryEndpointName)
                              .WithEnvironment(context =>
                              {
                                  context.EnvironmentVariables["SURREAL_USER"] = surrealDb.UserNameReference;
                                  context.EnvironmentVariables["SURREAL_PASS"] = surrealDb.PasswordParameter;
                              });
        //   .WithHealthCheck(healthCheckKey);

        return rabbitmq;
    }
}