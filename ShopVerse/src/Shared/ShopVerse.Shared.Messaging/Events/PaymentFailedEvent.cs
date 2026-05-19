using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Messaging.Events
{
    /// <summary>
    /// Bir siparişe ait ödemenin başarısız olduğunu temsil eden domain event'tir.
    /// Sipariş ID’si ve başarısızlık sebebi bilgisini içerir.
    /// </summary>
    public record PaymentFailedEvent : BaseDomainEvent
    {
        public Guid OrderId { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}
