namespace ShopVerse.Payment.Application.DTOs
{
    public record PaymentDto
    {
        public Guid Id { get; init; }
        public Guid OrderId { get; init; }
        public decimal Amount { get; init; }
        public string Status { get; init; } = string.Empty;
        public string? TransactionId { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
