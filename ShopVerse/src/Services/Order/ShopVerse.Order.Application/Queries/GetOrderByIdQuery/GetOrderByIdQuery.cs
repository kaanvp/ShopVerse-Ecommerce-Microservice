using MediatR;
using ShopVerse.Order.Application.DTOs;
using ShopVerse.Shared.Core;

namespace ShopVerse.Order.Application.Queries.GetOrderByIdQuery
{
    public class GetOrderByIdQuery : IRequest<Result<OrderDto>>
    {
        public Guid OrderId { get; set; }
    }
}
