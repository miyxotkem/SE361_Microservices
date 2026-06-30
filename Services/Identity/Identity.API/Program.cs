using BuildingBlocks.Security;
using Carter;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Microsoft.EntityFrameworkCore;
using Identity.API.Data;
using BuildingBlocks.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry Logging
builder.Logging.AddOpenTelemetryLogging(builder.Configuration);

// Add OpenTelemetry Tracing
builder.Services.AddOpenTelemetryTracing(builder.Configuration, "Identity.API");

// Register Supabase PostgreSQL DbContext
builder.Services.AddDbContext<IdentityDbContext>((sp, options) =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (builder.Configuration["UseInMemoryDatabase"] == "true")
    {
        var connection = sp.GetService<System.Data.Common.DbConnection>();
        if (connection != null)
        {
            options.UseSqlite(connection);
        }
        else
        {
            options.UseSqlite("DataSource=:memory:");
        }
    }
    else
    {
        options.UseNpgsql(connString);
    }
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
});

builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379", name: "redis")
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddGrpc();

// Add Carter & MediatR
var assembly = typeof(Program).Assembly;
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
    config.AddOpenBehavior(typeof(CachingBehavior<,>));
});

// Global Exception Handler
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

// Automatically Apply Migrations on Startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    if (db.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }
}

// Setup Pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.MapGrpcService<Identity.API.Services.UserGrpcService>();
app.UseExceptionHandler(options => { });
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
