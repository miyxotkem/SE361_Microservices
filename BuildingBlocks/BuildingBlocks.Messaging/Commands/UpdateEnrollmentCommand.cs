namespace BuildingBlocks.Messaging.Commands;

public record UpdateEnrollmentCommand
{
    public Guid CorrelationId { get; set; }
    public string UserId { get; set; } = default!;
    public string CourseId { get; set; } = default!;
    public string TransactionId { get; set; } = default!;
    public decimal Amount { get; set; }
}
