namespace ShopVerse.Shared.Messaging.Events
{
    /// <summary>
    /// Kargo durumunun güncellendiğini temsil eden domain event'tir.
    /// Kullanıcıya bildirim gönderilmesi için Notification servisine yayınlanır.
    /// </summary>
    public record CargoStatusUpdatedEvent : BaseDomainEvent
    {
        public Guid OrderId { get; init; }
        public Guid BuyerId { get; init; }
        public string TrackingNumber { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
    }
}
