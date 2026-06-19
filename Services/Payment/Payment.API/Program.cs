using BuildingBlocks.Messaging.MassTransit;
using Payment.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// Payment Services
builder.Services.AddScoped<IPaymentGatewayService, VnPayService>();
builder.Services.AddScoped<IPaymentGatewayService, MoMoService>();
builder.Services.AddScoped<IPaymentGatewayService, PayPalService>();

// Add Message Broker (RabbitMQ)
var assembly = typeof(Program).Assembly;
builder.Services.AddMessageBroker(builder.Configuration, assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
