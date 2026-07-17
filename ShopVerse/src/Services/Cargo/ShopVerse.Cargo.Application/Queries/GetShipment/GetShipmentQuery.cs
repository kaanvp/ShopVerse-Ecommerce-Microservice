using MediatR;
using ShopVerse.Cargo.Application.DTOs;
using ShopVerse.Shared.Core;

namespace ShopVerse.Cargo.Application.Queries.GetShipment
{
    public class GetShipmentQuery : IRequest<Result<ShipmentDto>>
    {
        public string TrackingNumber { get; set; } = string.Empty;
    }
}
