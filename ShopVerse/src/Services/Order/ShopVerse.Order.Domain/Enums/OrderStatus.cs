namespace ShopVerse.Order.Domain.Enums
{
    public enum OrderStatus
    {
        Created = 0,
        StockReserved = 1,
        Paid = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        Failed = 6
    }
}
