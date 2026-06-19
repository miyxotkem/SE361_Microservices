using BuildingBlocks.Messaging.Commands;
using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Course.API.Features.Registrations.Sagas;

public class EnrollmentStateMachine : MassTransitStateMachine<EnrollmentState>
{
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

    public EnrollmentStateMachine()
    {
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
                .TransitionTo(Completed),

            When(EnrollmentFailed)
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
