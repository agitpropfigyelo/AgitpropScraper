using Agitprop.AppHost;

using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging")
                       .WithManagementPlugin()
                       .WithOtlpExporter()
                       .PublishAsConnectionString();

// //TODO: clean this up
// IResourceBuilder<ContainerResource> surrealDb = builder.AddContainer("surrealdb", "surrealdb/surrealdb:latest")
//                        .WithArgs("start")
//                        .WithEnvironment("SURREAL_PASS", "root")
//                        .WithEnvironment("SURREAL_USER", "root")
//                        .WithEnvironment("SURREAL_PATH", "rocksdb:mydata/mydatabase.db")
//                        .WithBindMount("../databaseMount", "/mydata")
//                        .WithHttpEndpoint(targetPort: 8000)
//                        .WithOtlpExporter();

var surrealDb = builder.AddSurrealDB("surrealdb")
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
                      //   .WithEnvironment("surrealdbUrl", surrealDb.GetEndpoint("http"))
                      .WithReference(messaging)
                      .WaitFor(messaging)
                      .WaitFor(nlpService)
                      .WithEnvironment("nlpService", nlpService.GetEndpoint("http"))
                      .WithOtlpExporter()
                      .PublishAsDockerFile();
//    .WithReference(surrealDb)


var rssReader = builder.AddProject<Agitprop_RssFeedReader>("rss-feed-reader")
                       .WithReference(messaging)
                       .WaitFor(messaging)
                       .WithReference(consumer)
                       .WithOtlpExporter()
                       .PublishAsDockerFile();

builder.Build().Run();
