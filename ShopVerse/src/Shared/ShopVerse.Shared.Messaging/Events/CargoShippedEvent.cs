using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Messaging.Events
{
    /// <summary>
    /// Bir siparişin kargoya verildiğini temsil eden domain event'tir.
    /// Sipariş ID’si, takip numarası ve tahmini teslim tarihi bilgilerini içerir.
    /// </summary>
    public record CargoShippedEvent : BaseDomainEvent
    {
        public Guid OrderId { get; init; }
        public string TrackingNumber { get; init; } = string.Empty;
        public DateTime EstimatedDeliveryDate { get; init; }
    }
}
