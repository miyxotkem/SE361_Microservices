namespace BuildingBlocks.Messaging.Events;

public record CoursePurchasedEvent : IntegrationEvent
{
    public string UserId { get; set; } = default!;
    public string CourseId { get; set; } = default!;
    public string TransactionId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string? VoucherCode { get; set; }
}
