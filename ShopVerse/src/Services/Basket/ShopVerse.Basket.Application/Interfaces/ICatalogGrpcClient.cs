namespace ShopVerse.Basket.Application.Interfaces
{
    public class ProductInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public Guid CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public interface ICatalogGrpcClient
    {
        Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
    }
}
