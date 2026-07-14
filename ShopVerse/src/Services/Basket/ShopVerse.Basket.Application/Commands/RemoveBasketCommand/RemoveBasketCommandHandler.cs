using MediatR;
using ShopVerse.Basket.Application.Interfaces;
using ShopVerse.Shared.Core;
using Microsoft.AspNetCore.Http;

namespace ShopVerse.Basket.Application.Commands.RemoveBasketCommand
{
    public class RemoveBasketCommandHandler : IRequestHandler<RemoveBasketCommand, Result<Unit>>
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RemoveBasketCommandHandler(
            IBasketRepository basketRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _basketRepo = basketRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<Unit>> Handle(RemoveBasketCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Result<Unit>.Failure("User not authenticated.", 401);

            await _basketRepo.DeleteBasketAsync(userId, cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
