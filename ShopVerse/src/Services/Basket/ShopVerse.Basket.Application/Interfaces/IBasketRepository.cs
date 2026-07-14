using ShopVerse.Basket.Domain.Entity;

namespace ShopVerse.Basket.Application.Interfaces
{
    public interface IBasketRepository
    {
        Task<BasketCart?> GetBasketAsync(string userId, CancellationToken cancellationToken = default);
        Task SaveBasketAsync(string userId, BasketCart basket, CancellationToken cancellationToken = default);
        Task DeleteBasketAsync(string userId, CancellationToken cancellationToken = default);
    }
}
