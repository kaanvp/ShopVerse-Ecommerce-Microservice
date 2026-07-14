using MediatR;
using ShopVerse.Order.Application.DTOs;
using ShopVerse.Order.Application.Interfaces;
using ShopVerse.Shared.Core;

namespace ShopVerse.Order.Application.Queries.GetOrderByIdQuery
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result<OrderDto>.Failure("Order not found.", 404);

            var dto = MapToDto(order);
            return Result<OrderDto>.Success(dto);
        }

        private static OrderDto MapToDto(Domain.Entity.Order order)
        {
            return new OrderDto
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
            };
        }
    }
}
