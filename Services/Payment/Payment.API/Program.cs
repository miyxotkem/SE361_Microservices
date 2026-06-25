using BuildingBlocks.Messaging.MassTransit;
using Payment.API.Services;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

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

app.UseAuthorization();
app.MapControllers();

app.Run();
