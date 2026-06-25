using Microsoft.EntityFrameworkCore;
using Payment.API.Models;

namespace Payment.API.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<TransactionRecord> TransactionRecords { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TransactionRecord>(entity =>
            {
                entity.HasKey(t => t.TransactionId);
            });

            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.HasKey(v => v.Code);
            });

            // Seed default vouchers
            modelBuilder.Entity<Voucher>().HasData(
                new Voucher
                {
                    Code = "DISCOUNT20",
                    DiscountPercentage = 20,
                    MaxDiscountAmount = 100000,
                    ExpiryDate = new DateTime(2036, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                }
            );
        }
    }
}
