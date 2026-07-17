using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Messaging.Commands.Cargo
{
    /// <summary>
    /// Kargo gönderimi oluşturmak için kullanılan mesaj modelidir.
    /// Siparişe ait teslimat bilgilerini (adres, şehir, ilçe, posta kodu) içerir.
    /// </summary>
    public record CreateShipmentMessage
    {
        public Guid OrderId { get; init; }
        public Guid BuyerId { get; init; }
        public string ShippingAddress { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string District { get; init; } = string.Empty;
        public string ZipCode { get; init; } = string.Empty;
    }
}
