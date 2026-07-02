using MediatR;
using ShopVerse.Catalog.Application.DTOs;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Queries.GetProductByIdQuery
{
    public record GetProductByIdQuery : IRequest<Result<ProductDto>>
    {
        public Guid Id { get; init; }
    }
}
