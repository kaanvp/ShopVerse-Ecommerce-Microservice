using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Messaging.Events
{
    /// <summary>
    /// Kullanıcıya bildirim gönderilmesini tetikleyen domain event'tir.
    /// Bildirim alacak kullanıcı, başlık, mesaj ve bildirim tipi (ör: Order, Payment, Cargo) bilgilerini içerir.
    /// </summary>
    public record NotificationRequestedEvent : BaseDomainEvent
    {
        public Guid UserId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string Type { get; init; } = "Info"; // Order, Payment, Cargo vb.
    }
}
