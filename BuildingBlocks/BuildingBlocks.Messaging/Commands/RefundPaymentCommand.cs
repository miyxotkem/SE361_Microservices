namespace BuildingBlocks.Messaging.Commands;

public record RefundPaymentCommand
{
    public Guid CorrelationId { get; set; }
    public string TransactionId { get; set; } = default!;
    public string Reason { get; set; } = default!;
}
