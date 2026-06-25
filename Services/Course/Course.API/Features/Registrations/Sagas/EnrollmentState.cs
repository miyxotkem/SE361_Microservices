using MassTransit;

namespace Course.API.Features.Registrations.Sagas;

public class EnrollmentState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = default!;
    
    // Properties specific to this Saga
    public string UserId { get; set; } = default!;
    public string CourseId { get; set; } = default!;
    public string TransactionId { get; set; } = default!;
    public decimal Amount { get; set; }

    // Expiration token for timeout scheduling
    public Guid? ExpirationTokenId { get; set; }

    // Required by MassTransit.Redis
    public int Version { get; set; }
}
