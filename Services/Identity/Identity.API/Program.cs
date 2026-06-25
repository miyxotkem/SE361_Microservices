using BuildingBlocks.Security;
using Carter;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using Microsoft.EntityFrameworkCore;
using Identity.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Register Supabase PostgreSQL DbContext
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
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
});

// Global Exception Handler
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

// Automatically Apply Migrations on Startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    db.Database.Migrate();
}

// Setup Pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();
app.MapGrpcService<Identity.API.Services.UserGrpcService>();
app.UseExceptionHandler(options => { });
app.MapHealthChecks("/health");

app.Run();
