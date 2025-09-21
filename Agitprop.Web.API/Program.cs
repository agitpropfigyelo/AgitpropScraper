using Agitprop.Web.Api.Services;
using Agitprop.Infrastructure.Postgres;
using OpenTelemetry.Trace;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddServiceDiscovery();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddNewsfeedRepositories();
builder.Services.AddTransient<EntityService>();
builder.Services.AddControllers();

// OpenTelemetry Tracer registration (if not already present)
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder.AddAspNetCoreInstrumentation();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("ApplyMigrationsAtStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    Console.WriteLine("!!!!!!!!!!Applied migrations at startup!!!!!!!!!!");
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
