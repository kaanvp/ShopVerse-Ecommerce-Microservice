using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Messaging
{
    /// <summary>
    /// Domain event yapısının temel sözleşmesini ve base implementasyonunu tanımlar.
    /// EventId ve OccurredOn bilgileri ile domain içinde gerçekleşen olayların izlenmesini sağlar.
    /// </summary>
    public interface IDomainEvent
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
    }

    // Temel uygulama için abstract class veya record kullanılabilir
    public abstract class BaseDomainEvent : IDomainEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
