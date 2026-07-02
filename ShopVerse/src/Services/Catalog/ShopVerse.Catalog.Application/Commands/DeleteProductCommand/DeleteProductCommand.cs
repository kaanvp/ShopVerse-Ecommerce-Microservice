using MediatR;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Commands.DeleteProductCommand
{
    public record DeleteProductCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; init; }
    }
}
