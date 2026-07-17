using Dapper;
using Npgsql;
using ShopVerse.Cargo.Application.Interfaces;
using ShopVerse.Cargo.Domain.Entity;
using ShopVerse.Cargo.Domain.Enums;

namespace ShopVerse.Cargo.Infrastructure.Data
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly string _connectionString;

        public ShipmentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private NpgsqlConnection CreateConnection() => new(_connectionString);

        public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT id, order_id, buyer_id, tracking_number, status, shipping_address,
                       city, district, zip_code, estimated_delivery, created_at
                FROM shipments
                WHERE tracking_number = @TrackingNumber";

            using var connection = CreateConnection();
            var row = await connection.QueryFirstOrDefaultAsync<ShipmentRow>(sql, new { TrackingNumber = trackingNumber });
            return row == null ? null : MapToEntity(row);
        }

        public async Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT id, order_id, buyer_id, tracking_number, status, shipping_address,
                       city, district, zip_code, estimated_delivery, created_at
                FROM shipments
                WHERE order_id = @OrderId";

            using var connection = CreateConnection();
            var row = await connection.QueryFirstOrDefaultAsync<ShipmentRow>(sql, new { OrderId = orderId });
            return row == null ? null : MapToEntity(row);
        }

        public async Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT id, order_id, buyer_id, tracking_number, status, shipping_address,
                       city, district, zip_code, estimated_delivery, created_at
                FROM shipments
                WHERE id = @Id";

            using var connection = CreateConnection();
            var row = await connection.QueryFirstOrDefaultAsync<ShipmentRow>(sql, new { Id = id });
            return row == null ? null : MapToEntity(row);
        }

        public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO shipments (id, order_id, buyer_id, tracking_number, status, shipping_address,
                                       city, district, zip_code, estimated_delivery, created_at)
                VALUES (@Id, @OrderId, @BuyerId, @TrackingNumber, @Status, @ShippingAddress,
                        @City, @District, @ZipCode, @EstimatedDelivery, @CreatedAt)";

            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, new
            {
                shipment.Id,
                shipment.OrderId,
                shipment.BuyerId,
                shipment.TrackingNumber,
                Status = (int)shipment.Status,
                shipment.ShippingAddress,
                shipment.City,
                shipment.District,
                shipment.ZipCode,
                shipment.EstimatedDelivery,
                shipment.CreatedAt
            });
        }

        public async Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                UPDATE shipments
                SET status = @Status
                WHERE id = @Id";

            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, new
            {
                Status = (int)shipment.Status,
                shipment.Id
            });
        }

        private static Shipment MapToEntity(ShipmentRow row)
        {
            // Use reflection-free mapping via the factory — we need a way to reconstruct
            // the entity. Since constructor is private, we use the internal state map approach.
            return ShipmentMapper.FromRow(row);
        }

        // Internal DTO for Dapper
        public class ShipmentRow
        {
            public Guid id { get; set; }
            public Guid order_id { get; set; }
            public Guid buyer_id { get; set; }
            public string tracking_number { get; set; } = string.Empty;
            public int status { get; set; }
            public string shipping_address { get; set; } = string.Empty;
            public string city { get; set; } = string.Empty;
            public string district { get; set; } = string.Empty;
            public string zip_code { get; set; } = string.Empty;
            public DateTime estimated_delivery { get; set; }
            public DateTime created_at { get; set; }
        }
    }
}
