using MediatR;
using ShopVerse.Catalog.Application.DTOs;
using ShopVerse.Catalog.Domain.Interface;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Queries.GetCategoriesQuery
{
    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);

            var categoryDtos = categories
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ParentCategoryId = x.ParentCategoryId,
                    IsActive = x.IsActive
                })
                .ToList();

            return Result<List<CategoryDto>>.Success(categoryDtos);
        }
    }
}
