using BuildingBlocks.Messaging.MassTransit;
using Payment.API.Services;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using BuildingBlocks.Security;
using BuildingBlocks.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry Tracing
builder.Services.AddOpenTelemetryTracing(builder.Configuration, "Payment.API");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
});
builder.Services.AddHttpClient();

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Register Supabase PostgreSQL DbContext
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Payment Services
builder.Services.AddScoped<IPaymentGatewayService, VnPayService>();
builder.Services.AddScoped<IPaymentGatewayService, MoMoService>();
builder.Services.AddScoped<IPaymentGatewayService, PayPalService>();

// Add Message Broker (RabbitMQ)
var assembly = typeof(Program).Assembly;
builder.Services.AddMessageBroker(builder.Configuration, assembly);

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRabbitMQ(builder.Configuration);

var app = builder.Build();

// Automatically Apply Migrations on Startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
