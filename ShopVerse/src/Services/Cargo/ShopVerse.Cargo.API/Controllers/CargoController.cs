using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Cargo.Application.Commands.UpdateShipmentStatus;
using ShopVerse.Cargo.Application.Queries.GetShipment;
using ShopVerse.Cargo.Application.Queries.GetShipmentByOrder;
using ShopVerse.Cargo.Domain.Enums;

namespace ShopVerse.Cargo.API.Controllers
{
    [Route("api/v1/cargo")]
    [ApiController]
    [Authorize]
    public class CargoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CargoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET /api/v1/cargo/{trackingNumber}
        [HttpGet("{trackingNumber}")]
        public async Task<IActionResult> GetByTrackingNumber(string trackingNumber)
        {
            var result = await _mediator.Send(new GetShipmentQuery { TrackingNumber = trackingNumber });
            return StatusCode(result.StatusCode, result.IsSuccess ? result.Data : result.Error);
        }

        // GET /api/v1/cargo/order/{orderId}
        [HttpGet("order/{orderId:guid}")]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var result = await _mediator.Send(new GetShipmentByOrderQuery { OrderId = orderId });
            return StatusCode(result.StatusCode, result.IsSuccess ? result.Data : result.Error);
        }

        // PUT /api/v1/cargo/{id}/status
        [HttpPut("{id:guid}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            var result = await _mediator.Send(new UpdateShipmentStatusCommand
            {
                ShipmentId = id,
                NewStatus = request.Status
            });
            return StatusCode(result.StatusCode, result.IsSuccess ? result.Data : result.Error);
        }
    }

    public class UpdateStatusRequest
    {
        public ShipmentStatus Status { get; set; }
    }
}
