using MediatR;
using ShopVerse.Catalog.Domain.Interface;
using ShopVerse.Catalog.Application.Interfaces;
using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Application.Commands.DeleteProductCommand
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<Guid>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICatalogCacheService _cacheService;

        public DeleteProductCommandHandler(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            ICatalogCacheService cacheService)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<Result<Guid>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null)
                return Result<Guid>.Failure("Ürün bulunamadı.", 404);

            await _productRepository.DeleteAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Ürün silindi, hem tekil cache hem list cache temizlenir
            await _cacheService.RemoveAsync(product.Id);
            await _cacheService.RemoveListAsync();

            return Result<Guid>.Success(product.Id);
        }
    }
}
