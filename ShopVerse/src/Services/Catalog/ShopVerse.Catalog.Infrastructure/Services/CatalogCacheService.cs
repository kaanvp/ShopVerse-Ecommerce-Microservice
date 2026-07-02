using StackExchange.Redis;
using System.Text.Json;
using ShopVerse.Catalog.Application.DTOs;
using ShopVerse.Catalog.Application.Interfaces;

namespace ShopVerse.Catalog.Infrastructure.Services
{
    public class CatalogCacheService : ICatalogCacheService
    {
        private readonly IDatabase _redisDb;

        private const string CacheKeyPrefix = "product:";
        private const string ListCacheKey = "products:list";
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public CatalogCacheService(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        // --- Tekil ürün cache ---

        public async Task<ProductDto?> GetAsync(Guid id)
        {
            var key = $"{CacheKeyPrefix}{id}";
            var cachedData = await _redisDb.StringGetAsync(key);

            if (cachedData.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<ProductDto>(cachedData!, JsonOptions);
        }

        public async Task SetAsync(ProductDto product)
        {
            var key = $"{CacheKeyPrefix}{product.Id}";
            var serializedData = JsonSerializer.SerializeToUtf8Bytes(product, JsonOptions);

            await _redisDb.StringSetAsync(key, serializedData, CacheExpiry);
        }

        public async Task RemoveAsync(Guid id)
        {
            var key = $"{CacheKeyPrefix}{id}";
            await _redisDb.KeyDeleteAsync(key);
        }

        // --- Ürün listesi cache ---

        public async Task<IReadOnlyList<ProductDto>?> GetListAsync()
        {
            var cachedData = await _redisDb.StringGetAsync(ListCacheKey);

            if (cachedData.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<List<ProductDto>>(cachedData!, JsonOptions);
        }

        public async Task SetListAsync(IReadOnlyList<ProductDto> products)
        {
            var serializedData = JsonSerializer.SerializeToUtf8Bytes(products, JsonOptions);
            await _redisDb.StringSetAsync(ListCacheKey, serializedData, CacheExpiry);
        }

        public async Task RemoveListAsync()
        {
            await _redisDb.KeyDeleteAsync(ListCacheKey);
        }
    }
}
