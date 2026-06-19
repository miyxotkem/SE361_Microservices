namespace BuildingBlocks.Messaging.Events;

public record EnrollmentFailedEvent : IntegrationEvent
{
    public Guid CorrelationId { get; set; }
    public string Reason { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string CourseId { get; set; } = default!;
}
