using MediatR;
using ShopVerse.Catalog.Domain.Interface;
using ShopVerse.Catalog.Application.Interfaces;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Commands.UpdateProductCommand
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<Guid>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatalogCacheService _cacheService;

        public UpdateProductCommandHandler(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            ICatalogCacheService cacheService)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<Result<Guid>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null)
                return Result<Guid>.Failure("Ürün bulunamadı.", 404);

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Stock = request.Stock;
            product.CategoryId = request.CategoryId;
            product.ImageUrl = request.ImageUrl;
            product.IsActive = request.IsActive;

            await _productRepository.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Ürün güncellendi, hem tekil cache hem list cache temizlenir
            await _cacheService.RemoveAsync(product.Id);
            await _cacheService.RemoveListAsync();

            return Result<Guid>.Success(product.Id);
        }
    }
}
