using BuildingBlocks.Messaging.MassTransit;
using BuildingBlocks.Security;
using Carter;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry Tracing
builder.Services.AddOpenTelemetryTracing(builder.Configuration, "Admin.API");

builder.Services.AddHealthChecks()
    .AddRabbitMQ(builder.Configuration);

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Carter & MediatR
var assembly = typeof(Program).Assembly;
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Add Message Broker (RabbitMQ)
builder.Services.AddMessageBroker(builder.Configuration, assembly);

// Global Exception Handler
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

// Setup Pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.UseExceptionHandler(options => { });
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
