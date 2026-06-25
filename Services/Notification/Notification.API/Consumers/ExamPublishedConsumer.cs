using MassTransit;
using Google.Cloud.Firestore;
using BuildingBlocks.Messaging.Events;
using System.Threading.Tasks;

namespace Notification.API.Consumers
{
    public class ExamPublishedConsumer : IConsumer<ExamPublishedEvent>
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<ExamPublishedConsumer> _logger;
        private readonly Course.API.Grpc.CourseProtoService.CourseProtoServiceClient _courseProtoClient;

        public ExamPublishedConsumer(FirestoreDb firestoreDb, ILogger<ExamPublishedConsumer> logger, Course.API.Grpc.CourseProtoService.CourseProtoServiceClient courseProtoClient)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
            _courseProtoClient = courseProtoClient;
        }

        public async Task Consume(ConsumeContext<ExamPublishedEvent> context)
        {
            var @event = context.Message;
            _logger.LogInformation("Consuming ExamPublishedEvent for Exam: {ExamTitle} ({ExamId}) in Course: {CourseId}", 
                @event.Title, @event.ExamId, @event.CourseId);

            try
            {
                // Query accepted students in the course via gRPC
                var gRpcResponse = await _courseProtoClient.GetCourseStudentsAsync(
                    new Course.API.Grpc.GetCourseStudentsRequest { CourseId = @event.CourseId });

                if (gRpcResponse.StudentIds.Count == 0)
                {
                    _logger.LogInformation("No accepted students found in Course {CourseId}. Skipping notification creation.", @event.CourseId);
                    return;
                }

                var batch = _firestoreDb.StartBatch();
                var notifRef = _firestoreDb.Collection("Notifications");

                foreach (var studentId in gRpcResponse.StudentIds)
                {
                    var notifData = new Dictionary<string, object>
                    {
                        { "Title", "Bài kiểm tra mới!" },
                        { "Content", $"Giảng viên {@event.SenderName} đã công bố bài kiểm tra mới: {@event.Title}." },
                        { "TargetId", studentId },
                        { "CourseId", @event.CourseId },
                        { "Type", "Exam" },
                        { "SenderId", @event.SenderId },
                        { "SenderName", @event.SenderName },
                        { "IsRead", false },
                        { "CreatedAt", DateTime.UtcNow }
                    };
                    batch.Create(notifRef.Document(), notifData);
                }

                await batch.CommitAsync();
                _logger.LogInformation("Successfully created notifications for {Count} students in Course {CourseId}.", 
                    gRpcResponse.StudentIds.Count, @event.CourseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while consuming ExamPublishedEvent for Course {CourseId}.", @event.CourseId);
            }
        }
    }
}
