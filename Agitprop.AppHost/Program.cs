using Agitprop.AppHost;

using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging")
                       .WithManagementPlugin()
                       .WithOtlpExporter()
                       .PublishAsConnectionString();

var surrealDb = builder.AddSurrealDB("surrealdb")
                       .WithHttpEndpoint(port: 1289, targetPort: 8000, name: "SurrealistConnection")
                       .WithBindMount("../databaseMount", "/mydata")
                       .WithOtlpExporter();

#pragma warning disable ASPIREHOSTINGPYTHON001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
IResourceBuilder<Aspire.Hosting.Python.PythonAppResource> nlpService = builder.AddPythonApp("nlpService", "../Agitprop.NLPService", "app.py")
                        .WithHttpEndpoint(targetPort: 8111)
                        .WithHttpHealthCheck("/health", 200)
                        .WithOtlpExporter()
                        .PublishAsDockerFile();
#pragma warning restore ASPIREHOSTINGPYTHON001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

IResourceBuilder<ProjectResource> consumer = builder.AddProject<Agitprop_Consumer>("consumer")
                      .WaitFor(surrealDb)
                      .WithReference(surrealDb)
                      .WithReference(messaging)
                      .WaitFor(messaging)
                      .WaitFor(nlpService)
                      .WithEnvironment("nlpService", nlpService.GetEndpoint("http"))
                      .WithOtlpExporter()
                      .PublishAsDockerFile();

var rssReader = builder.AddProject<Agitprop_RssFeedReader>("rss-feed-reader")
                       .WithReference(messaging)
                       .WaitFor(messaging)
                       .WithReference(consumer)
                       .WithOtlpExporter()
                       .PublishAsDockerFile();

var backend = builder.AddProject<Agitprop_Web_Api>("backend")
                       .WithReference(surrealDb)
                       .WaitFor(surrealDb)
                       .PublishAsDockerFile();

builder.Build().Run();
