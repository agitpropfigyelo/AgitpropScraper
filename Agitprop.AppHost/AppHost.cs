using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging")
                       .WithManagementPlugin()
                       .WithOtlpExporter();

// var surrealDb = builder.AddSurrealDB("surrealdb")
//                        .WithHttpEndpoint(port: 1289, targetPort: 8000, name: "SurrealistConnection")
//                        .WithBindMount("../databaseMount", "/mydata")
//                        .WithOtlpExporter();
var postgres = builder.AddPostgres("postgres")
                      .WithDataVolume(isReadOnly: false)
                      .WithPgAdmin(pgAdmin =>
                      {
                          pgAdmin.WithHostPort(5050);
                          pgAdmin.WithImageTag("latest");
                      })
                      .WithLifetime(ContainerLifetime.Persistent)
                      .WithOtlpExporter();

var newsfeedDb = postgres.AddDatabase("newsfeed");

var nlpService = builder.AddUvicornApp("nlpService", "../Agitprop.Scraper.NLPService", "app:app")
                        .WithHttpHealthCheck("/health")
                        .WithOtlpExporter();


var proxyPool = builder.AddProject<Agitprop_Infrastructure_ProxyService>("proxy-pool")
                       .PublishAsDockerFile();

IResourceBuilder<ProjectResource> consumer = builder.AddProject<Agitprop_Scraper_Consumer>("consumer")
                      .WaitFor(newsfeedDb)
                      .WithReference(newsfeedDb)
                      .WaitFor(messaging)
                      .WithReference(messaging)
                      .WaitFor(nlpService)
                      .WithReference(nlpService)
                      .WithOtlpExporter()
                      .PublishAsDockerFile();

var rssReader = builder.AddProject<Agitprop_Scraper_RssFeedReader>("rss-feed-reader")
                       .WithReference(messaging)
                       .WaitFor(messaging)
                       .WaitFor(consumer)
                       .WithOtlpExporter()
                       .PublishAsDockerFile();

var backend = builder.AddProject<Agitprop_Web_Api>("backend")
                     .WaitFor(newsfeedDb)
                     .WithReference(newsfeedDb)
                     .WaitFor(messaging)
                     .WithReference(messaging)
                     .WithOtlpExporter()
                     .PublishAsDockerFile();

builder.AddNpmApp("angular", "../Agitprop.Web.Client")
    .WithReference(backend)
    .WaitFor(backend)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
