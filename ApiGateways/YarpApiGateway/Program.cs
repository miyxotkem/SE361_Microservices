var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseWebSockets();
app.MapHealthChecks("/health");
app.MapReverseProxy();

app.Run();
