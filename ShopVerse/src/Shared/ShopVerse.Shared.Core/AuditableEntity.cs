using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Core
{
    /// <summary>
    /// Oluşturulma ve güncellenme bilgilerini (audit bilgileri) içeren temel entity sınıfıdır.
    /// Tüm türetilen entity'ler için CreatedAt, CreatedBy, UpdatedAt ve UpdatedBy alanlarını standartlaştırır.
    /// </summary>
    public class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public String? CreatedBy { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public String? UpdatedBy { get; protected set; }
    }
}
