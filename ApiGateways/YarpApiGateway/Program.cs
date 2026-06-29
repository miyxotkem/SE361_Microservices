using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using BuildingBlocks.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenTelemetryTracing(builder.Configuration, "YarpApiGateway");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api-limiter", opt =>
    {
        opt.PermitLimit = 10000;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 10;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.RejectionStatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status429TooManyRequests;
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseWebSockets();
app.UseRateLimiter();
app.MapHealthChecks("/health");
app.MapReverseProxy();

app.Run();
