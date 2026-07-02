using MediatR;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Commands.CreateProductCommand
{
    public record CreateProductCommand : IRequest<Result<Guid>>
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public int Stock { get; init; }
        public Guid CategoryId { get; init; }
        public string? ImageUrl { get; init; }
    }
}
