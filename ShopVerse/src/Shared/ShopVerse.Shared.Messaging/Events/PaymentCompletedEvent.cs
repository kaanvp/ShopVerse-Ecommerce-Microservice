using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Messaging.Events
{
    /// <summary>
    /// Bir siparişe ait ödemenin başarıyla tamamlandığını temsil eden domain event'tir.
    /// Sipariş ID’si, işlem (transaction) numarası ve ödeme tutarı bilgilerini içerir.
    /// </summary>
    public record PaymentCompletedEvent : BaseDomainEvent
    {
        public Guid OrderId { get; init; }
        public string TransactionId { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }
}
