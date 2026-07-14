using Microsoft.EntityFrameworkCore;
using ShopVerse.Payment.Domain.Entity;

namespace ShopVerse.Payment.Infrastructure.Data
{
    public class PaymentDbContext : DbContext
    {
        public DbSet<Payment.Domain.Entity.Payment> Payments => Set<Payment.Domain.Entity.Payment>();
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment.Domain.Entity.Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => new { e.Status, e.CreatedAt });
            });
        }
    }
}
