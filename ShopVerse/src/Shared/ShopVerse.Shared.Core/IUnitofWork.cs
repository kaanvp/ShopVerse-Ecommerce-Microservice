using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Core
{
    /// <summary>
    /// Unit of Work patternini temsil eden arayüzdür.
    /// Veritabanı işlemlerinin tek bir transaction altında yönetilmesini sağlar
    /// ve yapılan değişiklikleri toplu olarak kaydetmek için kullanılır.
    /// </summary>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
