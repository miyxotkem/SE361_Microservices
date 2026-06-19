using BuildingBlocks.Messaging.Events;
using Course.API.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Course.API.EventBusConsumer;

public class EnrollmentStatusConsumer :
    IConsumer<EnrollmentActivatedEvent>,
    IConsumer<EnrollmentFailedEvent>
{
    private readonly IHubContext<EnrollmentHub> _hubContext;
    private readonly ILogger<EnrollmentStatusConsumer> _logger;

    public EnrollmentStatusConsumer(IHubContext<EnrollmentHub> hubContext, ILogger<EnrollmentStatusConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EnrollmentActivatedEvent> context)
    {
        _logger.LogInformation("EnrollmentActivatedEvent received for UserId: {UserId}, CourseId: {CourseId}", context.Message.UserId, context.Message.CourseId);

        var connectionId = EnrollmentHub.GetConnectionId(context.Message.UserId);
        if (!string.IsNullOrEmpty(connectionId))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("EnrollmentSuccess", new { CourseId = context.Message.CourseId });
        }
    }

    public async Task Consume(ConsumeContext<EnrollmentFailedEvent> context)
    {
        _logger.LogInformation("EnrollmentFailedEvent received for UserId: {UserId}, CourseId: {CourseId}, Reason: {Reason}", context.Message.UserId, context.Message.CourseId, context.Message.Reason);

        var connectionId = EnrollmentHub.GetConnectionId(context.Message.UserId);
        if (!string.IsNullOrEmpty(connectionId))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("EnrollmentFailed", new { CourseId = context.Message.CourseId, Reason = context.Message.Reason });
        }
    }
}
