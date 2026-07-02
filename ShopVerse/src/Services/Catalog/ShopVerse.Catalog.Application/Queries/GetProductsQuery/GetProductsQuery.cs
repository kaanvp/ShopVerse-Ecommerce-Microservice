using MediatR;
using ShopVerse.Catalog.Application.DTOs;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Queries.GetProductsQuery
{
    public record GetProductsQuery : IRequest<PaginatedResult<ProductDto>>
    {
        public ProductFilterDto Filter { get; init; } = new();
    }
}
