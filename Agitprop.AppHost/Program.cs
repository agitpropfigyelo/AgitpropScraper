using Aspire.Hosting;

using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging")
                       .WithManagementPlugin()
                       .WithOtlpExporter()
                       .PublishAsConnectionString();

var surrealDb = builder.AddContainer("surrealdb", "surrealdb/surrealdb:latest")
                       .WithArgs("start")
                       .WithOtlpExporter();

#pragma warning disable ASPIREHOSTINGPYTHON001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var nlpService = builder.AddPythonApp("nlpService", "../Agitprop.NLPService", "app.py")
                        .WithHttpEndpoint(targetPort:8111)
                        .WithHttpHealthCheck("/health", 200)
                        .WithOtlpExporter()
                        .PublishAsDockerFile();
#pragma warning restore ASPIREHOSTINGPYTHON001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var consumer = builder.AddProject<Agitprop_Consumer>("consumer")
                      .WaitFor(surrealDb)
                      .WithReference(messaging)
                      .WaitFor(messaging)
                      .WithReference(nlpService)
                      .WaitFor(nlpService)
                      .WithOtlpExporter()
                      .PublishAsDockerFile();
//    .WithReference(surrealDb)


var rssReader = builder.AddProject<Agitprop_RssFeedReader>("rss-feed-reader")
                       .WithReference(messaging)
                       .WaitFor(messaging)
                       .WithOtlpExporter()
                       .PublishAsDockerFile();

builder.Build().Run();
