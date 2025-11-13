using Agitprop.Infrastructure.ProxyService;

using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddServiceDiscovery();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI
builder.Services.AddSingleton<IProxyStore, InMemoryProxyStore>();
builder.Services.AddSingleton<IProxyValidator>(_ => new ProxyValidator(new Uri("https://www.vanenet.hu/"), TimeSpan.FromSeconds(6)));
builder.Services.AddSingleton<IProxyManager, ProxyManager>();
builder.Services.AddHostedService(sp => new ProxyRevalidatorBackgroundService(
    sp.GetRequiredService<IProxyManager>(), 
    TimeSpan.FromMinutes(15),
    sp.GetRequiredService<ILogger<ProxyRevalidatorBackgroundService>>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/proxies", (IProxyStore store) => Results.Ok(store.GetAll()));

app.MapGet("/proxy", async (IProxyManager mgr, string? strategy) =>
{
    var p = await mgr.GetProxyAsync(strategy ?? "random");
    return p is null ? Results.NoContent() : Results.Ok(p);
});

app.MapPost("/proxies/refresh", async (IProxyManager mgr, CancellationToken ct) =>
{
    await mgr.RefreshAllAsync(ct);
    return Results.Ok();
});

app.Run();
