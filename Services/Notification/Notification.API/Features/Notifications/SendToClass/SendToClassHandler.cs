using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Notification.API.Features.Notifications.SendToClass
{
    public class SendToClassRequest
    {
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? SenderId { get; set; }
        public string? SenderName { get; set; }
    }

    public record SendToClassCommand(string Uid, SendToClassRequest Request) : ICommand<IResult>;

    public class SendToClassCommandHandler : ICommandHandler<SendToClassCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly Course.API.Grpc.CourseProtoService.CourseProtoServiceClient _courseProtoClient;

        public SendToClassCommandHandler(FirestoreDb firestoreDb, Course.API.Grpc.CourseProtoService.CourseProtoServiceClient courseProtoClient)
        {
            _firestoreDb = firestoreDb;
            _courseProtoClient = courseProtoClient;
        }

        public async Task<IResult> Handle(SendToClassCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var req = request.Request;
                // Query courseRegistrations via gRPC client
                var gRpcResponse = await _courseProtoClient.GetCourseStudentsAsync(
                    new Course.API.Grpc.GetCourseStudentsRequest { CourseId = req.CourseId },
                    cancellationToken: cancellationToken);

                var batch = _firestoreDb.StartBatch();
                var notifRef = _firestoreDb.Collection("Notifications");

                foreach (var studentId in gRpcResponse.StudentIds)
                {
                    var notifData = new Dictionary<string, object>
                    {
                        { "Title", req.Title },
                        { "Content", req.Content },
                        { "TargetId", studentId },
                        { "CourseId", req.CourseId },
                        { "Type", req.Type ?? "Course" },
                        { "SenderId", string.IsNullOrEmpty(req.SenderId) ? request.Uid : req.SenderId },
                        { "SenderName", req.SenderName ?? "" },
                        { "IsRead", false },
                        { "CreatedAt", DateTime.UtcNow }
                    };
                    batch.Create(notifRef.Document(), notifData);
                }

                await batch.CommitAsync(cancellationToken);
                return Results.Ok(new { Message = "Notifications sent to class." });
            }
            catch (Exception ex)
            {
                return Results.Json(new { Message = "Error sending class notification", Error = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
