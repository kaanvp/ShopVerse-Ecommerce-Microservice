using MediatR;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Commands.CreateCategoryCommand
{
    public record CreateCategoryCommand : IRequest<Result<Guid>>
    {
        public string Name { get; init; } = string.Empty;
        public Guid? ParentCategoryId { get; init; }
    }
}
