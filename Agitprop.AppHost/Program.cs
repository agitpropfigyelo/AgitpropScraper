using Aspire.Hosting;

using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging")
                       .WithManagementPlugin();

var surrealDb = builder.AddContainer("surrealdb", "surrealdb/surrealdb:latest")
                       .WithVolume("./mydata:/mydata")
                       .WithArgs("start --user root --pass root file:/mydata/mydatabase.db");

#pragma warning disable ASPIREHOSTINGPYTHON001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var nlpService = builder.AddPythonApp("nlp-service", "../Agitprop.NLPService", "app.py")
                        .WithHttpEndpoint(env: "PORT")
                        .WithExternalHttpEndpoints()
                        .WithOtlpExporter();
#pragma warning restore ASPIREHOSTINGPYTHON001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var consumer = builder.AddProject<Agitprop_Consumer>("consumer")
                      .WithReference(surrealDb)
                      .WaitFor(surrealDb)
                      .WithReference(messaging)
                      .WaitFor(messaging)
                      .WithReference(nlpService)
                      .WaitFor(nlpService);

var rssReader = builder.AddProject<Agitprop_RssFeedReader>("rss-feed-reader")
                       .WithReference(messaging)
                       .WaitFor(messaging);

builder.Build().Run();
