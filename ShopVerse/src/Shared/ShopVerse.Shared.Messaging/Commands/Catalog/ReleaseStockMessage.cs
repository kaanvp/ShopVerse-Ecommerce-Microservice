namespace ShopVerse.Shared.Messaging.Commands.Catalog
{
    /// <summary>
    /// Ödeme başarısız olduğunda stok rezervasyonunu serbest bırakmak için kullanılan mesaj modelidir.
    /// Siparişe ait ürünlerin stoklarının geri verilmesini tetikler.
    /// </summary>
    public record ReleaseStockMessage
    {
        public Guid OrderId { get; init; }
        public List<StockItemDto> Items { get; init; } = new();
    }
}
