using System.Collections.Generic;
using System.Data.Common;
using Payment.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Payment.API.IntegrationTests;

public class PaymentApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Eagerly override WebApplicationBuilder configurations using builder.UseSetting
        builder.UseSetting("UseInMemoryDatabase", "true");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "DataSource=:memory:");
        builder.UseSetting("MessageBroker:Host", "rabbitmq://localhost");
        builder.UseSetting("Redis:ConnectionString", "localhost:6379");

        // Register DbConnection as singleton
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<DbConnection>(_connection);
        });

        builder.UseEnvironment("Development");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }
}
