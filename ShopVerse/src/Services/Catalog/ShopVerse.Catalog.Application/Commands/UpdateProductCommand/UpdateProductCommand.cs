using MediatR;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Commands.UpdateProductCommand
{
    public record UpdateProductCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public int Stock { get; init; }
        public Guid CategoryId { get; init; }
        public string? ImageUrl { get; init; }
        public bool IsActive { get; init; } = true;
    }
}
