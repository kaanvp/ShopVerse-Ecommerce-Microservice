using MediatR;
using ShopVerse.Cargo.Domain.Enums;
using ShopVerse.Shared.Core;

namespace ShopVerse.Cargo.Application.Commands.UpdateShipmentStatus
{
    public class UpdateShipmentStatusCommand : IRequest<Result<Unit>>
    {
        public Guid ShipmentId { get; set; }
        public ShipmentStatus NewStatus { get; set; }
    }
}
