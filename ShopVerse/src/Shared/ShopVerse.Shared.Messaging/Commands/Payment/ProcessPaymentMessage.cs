using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Messaging.Commands.Payment
{
    /// <summary>
    /// Ödeme işlemini başlatmak için kullanılan mesaj modelidir.
    /// Siparişe ait ödeme bilgilerini (OrderId, BuyerId, tutar ve ödeme yöntemi) içerir.
    /// </summary>
    public record ProcessPaymentMessage
    {
        public Guid OrderId { get; init; }
        public Guid BuyerId { get; init; }
        public decimal Amount { get; init; }
        public string PaymentMethod { get; init; } = "CreditCard";
    }
}
