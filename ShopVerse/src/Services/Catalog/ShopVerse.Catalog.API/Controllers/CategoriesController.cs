using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Catalog.Application.Commands.CreateCategoryCommand;
using ShopVerse.Catalog.Application.DTOs;
using ShopVerse.Catalog.Application.Queries.GetCategoriesQuery;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ISender _mediator;

        public CategoriesController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> GetAll()
        {
            var query = new GetCategoriesQuery();
            var result = await _mediator.Send(query);
            return Ok(result.Data);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guid>> Create(CreateCategoryCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(new { error = result.Error });

            return CreatedAtAction(nameof(GetAll), new { id = result.Data }, result.Data);
        }
    }
}
