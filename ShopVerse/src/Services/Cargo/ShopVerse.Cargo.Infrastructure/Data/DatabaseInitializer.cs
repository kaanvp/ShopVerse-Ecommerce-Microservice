using Dapper;
using Npgsql;

namespace ShopVerse.Cargo.Infrastructure.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(string connectionString)
        {
            const string createTableSql = @"
                CREATE TABLE IF NOT EXISTS shipments (
                    id              UUID            PRIMARY KEY,
                    order_id        UUID            NOT NULL,
                    buyer_id        UUID            NOT NULL,
                    tracking_number VARCHAR(50)     NOT NULL UNIQUE,
                    status          INTEGER         NOT NULL DEFAULT 0,
                    shipping_address VARCHAR(500)   NOT NULL DEFAULT '',
                    city            VARCHAR(100)    NOT NULL DEFAULT '',
                    district        VARCHAR(100)    NOT NULL DEFAULT '',
                    zip_code        VARCHAR(20)     NOT NULL DEFAULT '',
                    estimated_delivery TIMESTAMPTZ  NOT NULL,
                    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
                );

                CREATE INDEX IF NOT EXISTS idx_shipments_order_id        ON shipments(order_id);
                CREATE INDEX IF NOT EXISTS idx_shipments_tracking_number  ON shipments(tracking_number);
            ";

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            await connection.ExecuteAsync(createTableSql);
        }
    }
}
