using MediatR;
using Microsoft.AspNetCore.Http;
using ShopVerse.Order.Application.DTOs;
using ShopVerse.Order.Application.Interfaces;
using ShopVerse.Shared.Core;
using System.Security.Claims;

namespace ShopVerse.Order.Application.Queries.GetUserOrdersQuery
{
    public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, Result<List<OrderDto>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetUserOrdersQueryHandler(IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor)
        {
            _orderRepository = orderRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<List<OrderDto>>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var buyerId))
                return Result<List<OrderDto>>.Failure("User not authenticated.", 401);

            var orders = await _orderRepository.GetOrdersByBuyerIdAsync(buyerId, cancellationToken);

            var dtos = orders.Select(order => new OrderDto
            {
                Id = order.Id,
                BuyerId = order.BuyerId,
                Status = order.Status,
                TotalPrice = order.TotalPrice,
                ShippingAddress = order.ShippingAddress is not null ? new ShippingAddressDto
                {
                    FullName = order.ShippingAddress.FullName,
                    City = order.ShippingAddress.City,
                    District = order.ShippingAddress.District,
                    AddressLine = order.ShippingAddress.AddressLine,
                    ZipCode = order.ShippingAddress.ZipCode
                } : null,
                Items = order.OrderItems.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            }).ToList();

            return Result<List<OrderDto>>.Success(dtos);
        }
    }
}
