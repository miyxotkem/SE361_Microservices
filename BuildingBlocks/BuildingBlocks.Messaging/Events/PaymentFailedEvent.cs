namespace BuildingBlocks.Messaging.Events;

public record PaymentFailedEvent : IntegrationEvent
{
    public Guid CorrelationId { get; set; }
    public string Reason { get; set; } = default!;
}
