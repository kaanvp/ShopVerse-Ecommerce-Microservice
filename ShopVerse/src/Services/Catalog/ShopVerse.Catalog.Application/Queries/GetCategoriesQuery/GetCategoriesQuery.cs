using MediatR;
using ShopVerse.Catalog.Application.DTOs;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Queries.GetCategoriesQuery
{
    public record GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>;
}
