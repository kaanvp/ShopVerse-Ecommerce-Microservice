using MediatR;
using ShopVerse.Order.Application.DTOs;
using ShopVerse.Shared.Core;

namespace ShopVerse.Order.Application.Commands.CreateOrderCommand
{
    public class CreateOrderCommand : IRequest<Result<OrderDto>>
    {
        public ShippingAddressDto ShippingAddress { get; set; } = new();
    }
}
