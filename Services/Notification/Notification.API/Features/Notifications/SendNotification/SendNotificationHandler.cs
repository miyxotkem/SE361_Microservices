using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Notification.API.Features.Notifications.SendNotification
{
    public class SendNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public string? CourseId { get; set; }
        public string? Type { get; set; }
        public string? SenderId { get; set; }
        public string? SenderName { get; set; }
    }

    public record SendNotificationCommand(string Uid, SendNotificationRequest Request) : ICommand<IResult>;

    public class SendNotificationCommandHandler : ICommandHandler<SendNotificationCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public SendNotificationCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var req = request.Request;
                var notifData = new Dictionary<string, object>
                {
                    { "Title", req.Title },
                    { "Content", req.Content },
                    { "TargetId", req.TargetId },
                    { "CourseId", req.CourseId ?? "" },
                    { "Type", req.Type ?? "System" },
                    { "SenderId", string.IsNullOrEmpty(req.SenderId) ? request.Uid : req.SenderId },
                    { "SenderName", req.SenderName ?? "" },
                    { "IsRead", false },
                    { "CreatedAt", DateTime.UtcNow }
                };

                await _firestoreDb.Collection("Notifications").AddAsync(notifData, cancellationToken: cancellationToken);
                return Results.Ok(new { Message = "Notification sent." });
            }
            catch (Exception ex)
            {
                return Results.Json(new { Message = "Error sending notification", Error = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
