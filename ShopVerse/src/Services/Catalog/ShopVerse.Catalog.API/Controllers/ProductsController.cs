using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Catalog.Application.Commands.CreateProductCommand;
using ShopVerse.Catalog.Application.Commands.DeleteProductCommand;
using ShopVerse.Catalog.Application.Commands.UpdateProductCommand;
using ShopVerse.Catalog.Application.DTOs;
using ShopVerse.Catalog.Application.Queries.GetProductByIdQuery;
using ShopVerse.Catalog.Application.Queries.GetProductsQuery;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ISender _mediator;

        public ProductsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<ProductDto>>> GetAll(
            [FromQuery] ProductFilterDto filter)
        {
            var query = new GetProductsQuery { Filter = filter };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProductDto>> GetById(Guid id)
        {
            var query = new GetProductByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result.IsFailure)
                return NotFound(new { error = result.Error });

            return Ok(result.Data);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guid>> Create(CreateProductCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);
        }

        [HttpPut("{id:guid}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guid>> Update(Guid id, UpdateProductCommand command)
        {
            if (id != command.Id)
                return BadRequest(new { error = "Route ID ile body ID uyuşmuyor." });

            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return NotFound(new { error = result.Error });

            return Ok(result.Data);
        }

        [HttpDelete("{id:guid}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guid>> Delete(Guid id)
        {
            var command = new DeleteProductCommand { Id = id };
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return NotFound(new { error = result.Error });

            return Ok(result.Data);
        }
    }
}
