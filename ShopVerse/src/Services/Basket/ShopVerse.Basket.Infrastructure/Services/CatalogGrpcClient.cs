using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using Polly.Timeout;
using ShopVerse.Catalog.Grpc;
using Grpc.Net.Client;

using ShopVerse.Basket.Application.Interfaces;

namespace ShopVerse.Basket.Infrastructure.Services
{
    public class CatalogGrpcClient : ICatalogGrpcClient
    {
        private readonly CatalogGrpc.CatalogGrpcClient _client;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly AsyncTimeoutPolicy _timeoutPolicy;

        public CatalogGrpcClient(GrpcChannel channel)
        {
            _client = new CatalogGrpc.CatalogGrpcClient(channel);

            // Polly: Retry (3 kez, exponential backoff)
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            // Polly: Circuit Breaker (5 hata -> 30sn acik)
            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

            // Polly: Timeout (10sn)
            _timeoutPolicy = Policy.TimeoutAsync(10);
        }

        public async Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _timeoutPolicy.ExecuteAsync(async ct =>
                    await _retryPolicy.ExecuteAsync(async () =>
                        await _circuitBreakerPolicy.ExecuteAsync(async () =>
                        {
                            return await _client.GetProductByIdAsync(
                                new ProductRequest { Id = productId.ToString() },
                                cancellationToken: ct);
                        })),
                    cancellationToken);

                return new ProductInfo
                {
                    Id = Guid.Parse(response.Id),
                    Name = response.Name,
                    Description = response.Description,
                    Price = (decimal)response.Price,
                    Stock = response.Stock,
                    CategoryId = Guid.Parse(response.CategoryId),
                    ImageUrl = response.ImageUrl,
                    IsActive = response.IsActive
                };
            }
            catch
            {
                return null;
            }
        }
    }
}