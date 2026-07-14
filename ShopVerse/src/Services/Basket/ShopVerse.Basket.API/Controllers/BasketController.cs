using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Basket.Application.Commands.AddToBasketCommand;
using ShopVerse.Basket.Application.Commands.RemoveBasketCommand;
using ShopVerse.Basket.Application.Queries;

namespace ShopVerse.Basket.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BasketController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BasketController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetBasket()
        {
            var result = await _mediator.Send(new GetBasketQuery());
            return StatusCode(result.StatusCode, result.IsSuccess ? result.Data : result.Error);
        }

        [HttpPost]
        public async Task<IActionResult> AddToBasket([FromBody] AddToBasketCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result.IsSuccess ? result.Data : result.Error);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveBasket()
        {
            var result = await _mediator.Send(new RemoveBasketCommand());
            return StatusCode(result.StatusCode, result.IsSuccess ? result.Data : result.Error);
        }
    }
}
