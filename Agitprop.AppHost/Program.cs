using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging")
                       .WithManagementPlugin();

var consumer = builder.AddProject<Agitprop_Consumer>("consumer")
                      .WithReference(messaging)
                      .WaitFor(messaging);

var rssReader = builder.AddProject<Agitprop_RssFeedReader>("rss-feed-reader")
                       .WithReference(messaging)
                       .WaitFor(messaging);

builder.Build().Run();
