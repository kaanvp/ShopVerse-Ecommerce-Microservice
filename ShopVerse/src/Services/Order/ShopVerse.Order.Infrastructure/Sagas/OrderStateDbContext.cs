using Microsoft.EntityFrameworkCore;

namespace ShopVerse.Order.Infrastructure.Sagas
{
    public class OrderStateDbContext : DbContext
    {
        public OrderStateDbContext(DbContextOptions<OrderStateDbContext> options) : base(options) { }

        public DbSet<OrderState> OrderStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderState>(entity =>
            {
                entity.ToTable("SagaOrderStates");
                entity.HasKey(x => x.CorrelationId);

                entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired();
                entity.Property(x => x.OrderId).IsRequired();
                entity.Property(x => x.BuyerId).IsRequired();
                entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
                entity.Property(x => x.PaymentMethod).HasMaxLength(50);

                // Shipping address
                entity.Property(x => x.ShippingAddressFullName).HasMaxLength(200);
                entity.Property(x => x.ShippingCity).HasMaxLength(100);
                entity.Property(x => x.ShippingDistrict).HasMaxLength(100);
                entity.Property(x => x.ShippingAddressLine).HasMaxLength(500);
                entity.Property(x => x.ShippingZipCode).HasMaxLength(20);

                // RowVersion for optimistic concurrency
                entity.Property(x => x.RowVersion)
                    .IsRowVersion()
                    .HasColumnType("rowversion");
            });
        }
    }
}
