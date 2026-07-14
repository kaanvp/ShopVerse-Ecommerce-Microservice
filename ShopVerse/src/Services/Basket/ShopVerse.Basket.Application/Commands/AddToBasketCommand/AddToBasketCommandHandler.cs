using MediatR;
using ShopVerse.Basket.Domain.Entity;
using ShopVerse.Basket.Application.Interfaces;
using ShopVerse.Shared.Core;
using Microsoft.AspNetCore.Http;

namespace ShopVerse.Basket.Application.Commands.AddToBasketCommand
{
    public class AddToBasketCommandHandler : IRequestHandler<AddToBasketCommand, Result<Unit>>
    {
        private readonly IBasketRepository _basketRepo;
        private readonly ICatalogGrpcClient _catalogGrpc;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AddToBasketCommandHandler(
            IBasketRepository basketRepo,
            ICatalogGrpcClient catalogGrpc,
            IHttpContextAccessor httpContextAccessor)
        {
            _basketRepo = basketRepo;
            _catalogGrpc = catalogGrpc;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<Unit>> Handle(AddToBasketCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Result<Unit>.Failure("User not authenticated.", 401);

            // gRPC ile Catalog'dan urun bilgisini dogrula
            var product = await _catalogGrpc.GetProductAsync(request.ProductId, cancellationToken);
            if (product == null)
                return Result<Unit>.Failure("Product not found.", 404);

            if (product.Stock < request.Quantity)
                return Result<Unit>.Failure("Insufficient stock.", 400);

            // Sepeti al veya yeni olustur
            var basket = await _basketRepo.GetBasketAsync(userId, cancellationToken)
                         ?? new BasketCart { UserId = userId };

            basket.AddItem(new BasketItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = request.Quantity
            });

            await _basketRepo.SaveBasketAsync(userId, basket, cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
