using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging")
                       .WithManagementPlugin()
                       .WithOtlpExporter()
                       .PublishAsConnectionString();

// var surrealDb = builder.AddSurrealDB("surrealdb")
//                        .WithHttpEndpoint(port: 1289, targetPort: 8000, name: "SurrealistConnection")
//                        .WithBindMount("../databaseMount", "/mydata")
//                        .WithOtlpExporter();
var postgres = builder.AddPostgres("postgres")
                      .WithDataVolume(isReadOnly: false)
                      .WithPgAdmin(pgAdmin => {
                          pgAdmin.WithHostPort(5050);
                          pgAdmin.WithImageTag("latest");})
                      .WithOtlpExporter()
                      .WithLifetime(ContainerLifetime.Persistent)
                      .AddDatabase("newsfeed");

#pragma warning disable ASPIREHOSTINGPYTHON001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
IResourceBuilder<Aspire.Hosting.Python.PythonAppResource> nlpService = builder.AddPythonApp("nlpService", "../Agitprop.Scraper.NLPService", "app.py")
                        .WithHttpEndpoint(env: "PORT")
                        .WithHttpHealthCheck("/health", 200)
                        .WithExternalHttpEndpoints();
                        //.WithOtlpExporter()
                        // .PublishAsDockerFile();
#pragma warning restore ASPIREHOSTINGPYTHON001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.



IResourceBuilder<ProjectResource> consumer = builder.AddProject<Agitprop_Scraper_Consumer>("consumer")
                      .WaitFor(postgres)
                      .WithReference(postgres)
                      .WaitFor(messaging)
                      .WithReference(messaging)
                      .WaitFor(nlpService)
                      .WithEnvironment("nlpService", nlpService.GetEndpoint("http"))
                      .WithOtlpExporter()
                      .PublishAsDockerFile();

var rssReader = builder.AddProject<Agitprop_Scraper_RssFeedReader>("rss-feed-reader")
                       .WithReference(messaging)
                       .WaitFor(messaging)
                       .WaitFor(consumer)
                       .WithOtlpExporter()
                       .PublishAsDockerFile();

var backend = builder.AddProject<Agitprop_Web_Api>("backend")
                     .WaitFor(postgres)
                     .WithReference(postgres)
                     .PublishAsDockerFile();

builder.AddNpmApp("angular", "../Agitprop.Web.Client")
    .WithReference(backend)
    .WaitFor(backend)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
