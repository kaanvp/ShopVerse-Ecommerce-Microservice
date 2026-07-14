using ShopVerse.Shared.Core;

namespace ShopVerse.Notification.Domain.Entity
{
    public class Notification : AuditableEntity
    {
        public new Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info"; // Order, Payment, Cargo
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
