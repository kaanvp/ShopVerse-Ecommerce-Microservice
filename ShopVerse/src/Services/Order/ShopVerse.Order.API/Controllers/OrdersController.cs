using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Order.Application.Commands.CancelOrderCommand;
using ShopVerse.Order.Application.Commands.CreateOrderCommand;
using ShopVerse.Order.Application.Queries.GetOrderByIdQuery;
using ShopVerse.Order.Application.Queries.GetUserOrdersQuery;

namespace ShopVerse.Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Yeni sipariş oluşturur.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsFailure)
                return StatusCode(result.StatusCode, new { error = result.Error });

            return StatusCode(result.StatusCode, result.Data);
        }

        /// <summary>
        /// Sipariş ID'sine göre sipariş getirir.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var query = new GetOrderByIdQuery { OrderId = id };
            var result = await _mediator.Send(query);
            if (result.IsFailure)
                return StatusCode(result.StatusCode, new { error = result.Error });

            return Ok(result.Data);
        }

        /// <summary>
        /// Giriş yapmış kullanıcının tüm siparişlerini getirir.
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var query = new GetUserOrdersQuery();
            var result = await _mediator.Send(query);
            if (result.IsFailure)
                return StatusCode(result.StatusCode, new { error = result.Error });

            return Ok(result.Data);
        }

        /// <summary>
        /// Siparişi iptal eder.
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var command = new CancelOrderCommand { OrderId = id };
            var result = await _mediator.Send(command);
            if (result.IsFailure)
                return StatusCode(result.StatusCode, new { error = result.Error });

            return Ok(new { message = "Order cancelled successfully." });
        }
    }
}
