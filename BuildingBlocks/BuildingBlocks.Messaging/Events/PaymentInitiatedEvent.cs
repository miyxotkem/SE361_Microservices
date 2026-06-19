namespace BuildingBlocks.Messaging.Events;

public record PaymentInitiatedEvent : IntegrationEvent
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = default!;
    public string CourseId { get; set; } = default!;
    public decimal Amount { get; set; }
}
