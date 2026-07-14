using ShopVerse.Basket.Domain.Entity;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using ShopVerse.Basket.Application.Interfaces;

namespace ShopVerse.Basket.Infrastructure.Services
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDatabase _redis;
        private static readonly TimeSpan BasketTtl = TimeSpan.FromDays(7);
        private readonly string _basketKeyPrefix = "basket:";
        public BasketRepository(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }
        public async Task<BasketCart?> GetBasketAsync(string userId, CancellationToken cancellationToken = default)
        {
            var key = $"{_basketKeyPrefix}{userId}";
            var data = await _redis.StringGetAsync(key);
            if (data.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<BasketCart>(data!);
        }
        public async Task SaveBasketAsync(string userId,BasketCart basket,CancellationToken cancellationToken = default)
        {
            var key = $"{_basketKeyPrefix}{userId}";
            var data = JsonSerializer.Serialize(basket);
            await _redis.StringSetAsync(key, data, BasketTtl);
        }
        public async Task DeleteBasketAsync(string userId, CancellationToken cancellationToken = default)
        {
            var key = $"{_basketKeyPrefix}{userId}";
            await _redis.KeyDeleteAsync(key);
        }
    }
}
