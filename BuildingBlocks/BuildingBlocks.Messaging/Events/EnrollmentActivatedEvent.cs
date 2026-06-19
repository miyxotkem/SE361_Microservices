namespace BuildingBlocks.Messaging.Events;

public record EnrollmentActivatedEvent : IntegrationEvent
{
    public Guid CorrelationId { get; set; }
    public string UserId { get; set; } = default!;
    public string CourseId { get; set; } = default!;
}
