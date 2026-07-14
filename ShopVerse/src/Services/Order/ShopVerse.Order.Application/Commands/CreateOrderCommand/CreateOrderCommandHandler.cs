using MediatR;
using ShopVerse.Order.Application.DTOs;
using ShopVerse.Order.Application.Interfaces;
using ShopVerse.Order.Domain.Enums;
using ShopVerse.Order.Domain.ValueObjects;
using ShopVerse.Shared.Core;
using ShopVerse.Shared.Messaging.Events;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ShopVerse.Order.Application.Commands.CreateOrderCommand
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // 1. Kullanıcıyı al
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var buyerId))
                return Result<OrderDto>.Failure("User not authenticated.", 401);

            // 2. Shipping address value object'ini oluştur
            var shippingAddress = new ShippingAddress(
                request.ShippingAddress.FullName,
                request.ShippingAddress.City,
                request.ShippingAddress.District,
                request.ShippingAddress.AddressLine,
                request.ShippingAddress.ZipCode
            );

            // 3. Order entity'sini oluştur
            var order = new Domain.Entity.Order(buyerId, shippingAddress);

            // 4. Event ekle
            order.AddDomainEvent(new OrderCreatedEvent
            {
                OrderId = order.Id,
                BuyerId = buyerId,
                TotalAmount = order.TotalPrice,
                Items = order.OrderItems.Select(i => new ShopVerse.Shared.Messaging.Events.OrderItemDto(
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice
                )).ToList(),
                ShippingAddressFullName = request.ShippingAddress.FullName,
                ShippingCity = request.ShippingAddress.City,
                ShippingDistrict = request.ShippingAddress.District,
                ShippingAddressLine = request.ShippingAddress.AddressLine,
                ShippingZipCode = request.ShippingAddress.ZipCode
            });

            // 5. Kaydet
            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Response DTO
            var dto = MapToDto(order);
            return Result<OrderDto>.Success(dto, 201);
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
                Items = order.OrderItems.Select(i => new DTOs.OrderItemDto
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
