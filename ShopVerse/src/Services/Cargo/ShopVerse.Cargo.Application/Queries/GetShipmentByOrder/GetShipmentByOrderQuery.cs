using MediatR;
using ShopVerse.Cargo.Application.DTOs;
using ShopVerse.Shared.Core;

namespace ShopVerse.Cargo.Application.Queries.GetShipmentByOrder
{
    public class GetShipmentByOrderQuery : IRequest<Result<ShipmentDto>>
    {
        public Guid OrderId { get; set; }
    }
}
