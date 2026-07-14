using MediatR;
using ShopVerse.Basket.Application.DTOs;
using ShopVerse.Basket.Application.Interfaces;
using ShopVerse.Shared.Core;
using Microsoft.AspNetCore.Http;

namespace ShopVerse.Basket.Application.Queries
{
    public class GetBasketQueryHandler : IRequestHandler<GetBasketQuery, Result<BasketDto>>
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetBasketQueryHandler(
            IBasketRepository basketRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _basketRepo = basketRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<BasketDto>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Result<BasketDto>.Failure("User not authenticated.", 401);

            var basket = await _basketRepo.GetBasketAsync(userId, cancellationToken);
            if (basket == null)
                return Result<BasketDto>.Failure("Basket not found.", 404);

            var dto = new BasketDto
            {
                UserId = basket.UserId,
                Items = basket.Items.Select(i => new BasketItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList(),
                TotalPrice = basket.TotalPrice
            };

            return Result<BasketDto>.Success(dto);
        }
    }
}
