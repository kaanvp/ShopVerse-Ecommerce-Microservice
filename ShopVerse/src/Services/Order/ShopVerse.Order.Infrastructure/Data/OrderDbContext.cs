using Microsoft.EntityFrameworkCore;
using ShopVerse.Order.Domain.Entity;
using ShopVerse.Order.Domain.ValueObjects;

namespace ShopVerse.Order.Infrastructure.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Domain.Entity.Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Order tablosu
            modelBuilder.Entity<Domain.Entity.Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(o => o.Id);

                entity.Property(o => o.BuyerId).IsRequired();
                entity.Property(o => o.Status)
                    .HasConversion<string>()
                    .HasMaxLength(30)
                    .IsRequired();
                entity.Property(o => o.TotalPrice)
                    .HasPrecision(18, 2)
                    .IsRequired();

                // ShippingAddress - Value Object owned entity
                entity.OwnsOne(o => o.ShippingAddress, sa =>
                {
                    sa.Property(a => a.FullName).HasColumnName("ShippingFullName").HasMaxLength(200);
                    sa.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100);
                    sa.Property(a => a.District).HasColumnName("ShippingDistrict").HasMaxLength(100);
                    sa.Property(a => a.AddressLine).HasColumnName("ShippingAddressLine").HasMaxLength(500);
                    sa.Property(a => a.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20);
                });

                // OrderItems - one-to-many
                entity.HasMany(o => o.OrderItems)
                    .WithOne()
                    .HasForeignKey("OrderId")
                    .OnDelete(DeleteBehavior.Cascade);

                // Audit fields
                entity.Property(o => o.CreatedAt).IsRequired();
                entity.Property(o => o.CreatedBy).HasMaxLength(100);
                entity.Property(o => o.UpdatedBy).HasMaxLength(100);
            });

            // OrderItem tablosu
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(oi => oi.Id);

                entity.Property(oi => oi.ProductId).IsRequired();
                entity.Property(oi => oi.ProductName).HasMaxLength(200).IsRequired();
                entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2).IsRequired();
                entity.Property(oi => oi.Quantity).IsRequired();
            });

            // OutboxMessage tablosu
            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.ToTable("OutboxMessages");
                entity.HasKey(om => om.Id);

                entity.Property(om => om.EventType).HasMaxLength(200).IsRequired();
                entity.Property(om => om.Payload).IsRequired();
                entity.Property(om => om.CreatedAt).IsRequired();
                entity.Property(om => om.ProcessedAt);

                entity.HasIndex(om => om.ProcessedAt);
                entity.HasIndex(om => om.CreatedAt);
            });
        }
    }
}
