using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Messaging.Events
{
    /// <summary>
    /// Yeni bir sipariş oluşturulduğunda yayınlanan domain event'tir.
    /// Sipariş bilgilerini (OrderId, BuyerId, toplam tutar ve ürün listesi) içerir.
    /// </summary>
    public record OrderCreatedEvent : BaseDomainEvent
{
    public Guid OrderId { get; init; }
    public Guid BuyerId { get; init; }
    public decimal TotalAmount { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
}

// Basit DTO tanımı (Messaging katmanında bağımsız olmalı)
public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);
}
