using Agitprop.Infrastructure.SurrealDB;
using Agitprop.Infrastructure.SurrealDB.Models;
using Agitprop.Web.Api.Services;

using OpenTelemetry.Trace;

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

// builder.Services.AddScoped<EntityService>();
//builder.Services.AddScoped<IEntityRepository, EntityRepository>();
// builder.Services.AddScoped<ITrendingRepository, TrendingRepository>();
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

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
