namespace ShopVerse.Order.Domain.ValueObjects
{
    /// <summary>
    /// Sipariş teslimat adresi için value object.
    /// Immutable olup eşitlik karşılaştırması tüm property'ler üzerinden yapılır.
    /// </summary>
    public class ShippingAddress : IEquatable<ShippingAddress>
    {
        public string FullName { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string District { get; init; } = string.Empty;
        public string AddressLine { get; init; } = string.Empty;
        public string ZipCode { get; init; } = string.Empty;

        private ShippingAddress() { }

        public ShippingAddress(string fullName, string city, string district, string addressLine, string zipCode)
        {
            FullName = fullName;
            City = city;
            District = district;
            AddressLine = addressLine;
            ZipCode = zipCode;
        }

        public bool Equals(ShippingAddress? other)
        {
            if (other is null) return false;
            return FullName == other.FullName &&
                   City == other.City &&
                   District == other.District &&
                   AddressLine == other.AddressLine &&
                   ZipCode == other.ZipCode;
        }

        public override bool Equals(object? obj) => Equals(obj as ShippingAddress);

        public override int GetHashCode() =>
            HashCode.Combine(FullName, City, District, AddressLine, ZipCode);

        public static bool operator ==(ShippingAddress? left, ShippingAddress? right) =>
            Equals(left, right);

        public static bool operator !=(ShippingAddress? left, ShippingAddress? right) =>
            !Equals(left, right);
    }
}
