namespace BuildingBlocks.Messaging.Events;

public record PaymentCompletedEvent : IntegrationEvent
{
    public Guid CorrelationId { get; set; }
    public string TransactionId { get; set; } = default!;
}
