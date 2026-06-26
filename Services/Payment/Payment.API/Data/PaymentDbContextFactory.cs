using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Payment.API.Data
{
    public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
    {
        public PaymentDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Try to load connection string from config or environment variables
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? configuration["PAYMENT_DB_CONNECTION"]; // Support environment override

            var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new PaymentDbContext(optionsBuilder.Options);
        }
    }
}
