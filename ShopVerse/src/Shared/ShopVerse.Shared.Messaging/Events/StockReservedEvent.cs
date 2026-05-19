using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Messaging.Events
{
    /// <summary>
    /// Sipariş için stok rezervasyon işleminin sonucunu temsil eden domain event'tir.
    /// Rezervasyonun başarılı olup olmadığını ve başarısızsa sebebini içerir.
    /// </summary>
    public record StockReservedEvent : BaseDomainEvent
    {
        public Guid OrderId { get; init; }
        public bool IsSuccess { get; init; }
        public string? FailureReason { get; init; }
    }
}
