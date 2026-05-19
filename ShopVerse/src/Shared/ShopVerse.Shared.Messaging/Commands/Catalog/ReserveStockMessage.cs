using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Messaging.Commands.Catalog
{
    /// <summary>
    /// Sipariş için stok rezervasyonu yapılmasını tetikleyen mesaj modelidir.
    /// Siparişe ait ürünlerin ve miktarlarının stoktan ayrılması için kullanılır.
    /// </summary>
    public record ReserveStockMessage
    {
        public Guid OrderId { get; init; }
        public List<StockItemDto> Items { get; init; } = new();
    }

    public record StockItemDto(Guid ProductId, int Quantity);
}
