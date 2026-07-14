using ShopVerse.Shared.Core;

namespace ShopVerse.Payment.Application.Interfaces
{
    public interface IPaymentRepository : IRepository<Domain.Entity.Payment>
    {
        Task<IReadOnlyList<Domain.Entity.Payment>> GetPendingPaymentsOlderThanAsync(
            DateTime threshold, CancellationToken cancellationToken = default);

        Task<Domain.Entity.Payment?> GetByOrderIdAsync(
            Guid orderId, CancellationToken cancellationToken = default);
    }
}
