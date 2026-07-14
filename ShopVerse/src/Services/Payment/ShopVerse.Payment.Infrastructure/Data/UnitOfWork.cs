using ShopVerse.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Payment.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PaymentDbContext _context;
        public UnitOfWork(PaymentDbContext context) => _context = context;
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct);
    }
}