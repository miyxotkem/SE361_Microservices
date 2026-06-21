using BuildingBlocks.Messaging.Commands;
using BuildingBlocks.Messaging.Events;
using Course.API.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Course.API.Features.Registrations.Sagas;

public class EnrollmentStateMachine : MassTransitStateMachine<EnrollmentState>
{
    private readonly IHubContext<EnrollmentHub> _hubContext;

    // Define States
    public State PaymentProcessing { get; private set; } = default!;
    public State Enrolling { get; private set; } = default!;
    public State Completed { get; private set; } = default!;
    public State Failed { get; private set; } = default!;
    public State Compensating { get; private set; } = default!;

    // Define Events
    public Event<PaymentInitiatedEvent> PaymentInitiated { get; private set; } = default!;
    public Event<PaymentCompletedEvent> PaymentCompleted { get; private set; } = default!;
    public Event<PaymentFailedEvent> PaymentFailed { get; private set; } = default!;
    public Event<EnrollmentActivatedEvent> EnrollmentActivated { get; private set; } = default!;
    public Event<EnrollmentFailedEvent> EnrollmentFailed { get; private set; } = default!;

    public EnrollmentStateMachine(IHubContext<EnrollmentHub> hubContext)
    {
        _hubContext = hubContext;
        InstanceState(x => x.CurrentState);

        // Configure correlation for events
        Event(() => PaymentInitiated, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => PaymentCompleted, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => PaymentFailed, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => EnrollmentActivated, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => EnrollmentFailed, x => x.CorrelateById(context => context.Message.CorrelationId));

        // Define workflow
        Initially(
            When(PaymentInitiated)
                .Then(context =>
                {
                    context.Saga.UserId = context.Message.UserId;
                    context.Saga.CourseId = context.Message.CourseId;
                    context.Saga.Amount = context.Message.Amount;
                })
                .TransitionTo(PaymentProcessing)
        );

        During(PaymentProcessing,
            When(PaymentCompleted)
                .Then(context =>
                {
                    context.Saga.TransactionId = context.Message.TransactionId;
                })
                .SendAsync(new Uri("queue:update-enrollment"), context => context.Init<UpdateEnrollmentCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    UserId = context.Saga.UserId,
                    CourseId = context.Saga.CourseId,
                    TransactionId = context.Saga.TransactionId,
                    Amount = context.Saga.Amount
                }))
                .TransitionTo(Enrolling),

            When(PaymentFailed)
                .TransitionTo(Failed)
        );

        During(Enrolling,
            When(EnrollmentActivated)
                // Send SignalR notification directly from Saga (avoid competing consumer issue)
                .ThenAsync(async context =>
                {
                    var userId = context.Saga.UserId;
                    var courseId = context.Saga.CourseId;
                    var connectionId = EnrollmentHub.GetConnectionId(userId);
                    if (!string.IsNullOrEmpty(connectionId))
                    {
                        await _hubContext.Clients.Client(connectionId)
                            .SendAsync("EnrollmentSuccess", new { CourseId = courseId });
                    }
                })
                .TransitionTo(Completed),

            When(EnrollmentFailed)
                // Send SignalR failure notification directly from Saga
                .ThenAsync(async context =>
                {
                    var userId = context.Saga.UserId;
                    var courseId = context.Saga.CourseId;
                    var reason = context.Message.Reason;
                    var connectionId = EnrollmentHub.GetConnectionId(userId);
                    if (!string.IsNullOrEmpty(connectionId))
                    {
                        await _hubContext.Clients.Client(connectionId)
                            .SendAsync("EnrollmentFailed", new { CourseId = courseId, Reason = reason });
                    }
                })
                // If enrollment fails, issue a refund command
                .SendAsync(new Uri("queue:refund-payment"), context => context.Init<RefundPaymentCommand>(new
                {
                    CorrelationId = context.Saga.CorrelationId,
                    TransactionId = context.Saga.TransactionId,
                    Reason = context.Message.Reason
                }))
                .TransitionTo(Compensating)
        );
    }
}
