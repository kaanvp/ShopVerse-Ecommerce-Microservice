using MediatR;
using ShopVerse.Catalog.Application.Interfaces;
using ShopVerse.Catalog.Domain.Entity;
using ShopVerse.Catalog.Domain.Interface;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Commands.CreateProductCommand
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatalogCacheService _cacheService;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            ICatalogCacheService cacheService)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                CategoryId = request.CategoryId,
                ImageUrl = request.ImageUrl,
                IsActive = true
            };

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Yeni ürün eklendi, list cache güncelliğini yitirdi
            await _cacheService.RemoveListAsync();

            return Result<Guid>.Success(product.Id);
        }
    }
}
