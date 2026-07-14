using Microsoft.EntityFrameworkCore;
using ShopVerse.Payment.Application.Interfaces;
using ShopVerse.Payment.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Payment.Infrastructure.Data.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;
        public PaymentRepository(PaymentDbContext context) => _context = context;

        public async Task<Payment.Domain.Entity.Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Payments.FindAsync(new object[] { id }, ct);

        public async Task<IReadOnlyList<Payment.Domain.Entity.Payment>> GetAllAsync(CancellationToken ct = default)
            => await _context.Payments.ToListAsync(ct);

        public async Task AddAsync(Payment.Domain.Entity.Payment entity, CancellationToken ct = default)
            => await _context.Payments.AddAsync(entity, ct);

        public Task UpdateAsync(Payment.Domain.Entity.Payment entity, CancellationToken ct = default)
        {
            _context.Payments.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Payment.Domain.Entity.Payment entity, CancellationToken ct = default)
        {
            _context.Payments.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<Payment.Domain.Entity.Payment>> GetPendingPaymentsOlderThanAsync(
            DateTime threshold, CancellationToken ct = default)
            => await _context.Payments
                .Where(p => p.Status == PaymentStatus.Pending && p.CreatedAt <= threshold)
                .ToListAsync(ct);

        public async Task<Payment.Domain.Entity.Payment?> GetByOrderIdAsync(
            Guid orderId, CancellationToken ct = default)
            => await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId, ct);
    }
}
