using Aspire.Hosting;

using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging");

builder.AddProject<Agitprop_Consumer>("consumer")
       .WithReference(messaging);

builder.Build().Run();
