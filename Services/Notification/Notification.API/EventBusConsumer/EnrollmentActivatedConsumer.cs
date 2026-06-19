using BuildingBlocks.Messaging.Events;
using Google.Cloud.Firestore;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Notification.API.EventBusConsumer
{
    public class EnrollmentActivatedConsumer : IConsumer<EnrollmentActivatedEvent>
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<EnrollmentActivatedConsumer> _logger;

        public EnrollmentActivatedConsumer(FirestoreDb firestoreDb, ILogger<EnrollmentActivatedConsumer> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EnrollmentActivatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Integration Event consumed: {EventId} - Sending Notification for Payment of Course: {CourseId}",
                message.Id, message.CourseId);

            var notificationRef = _firestoreDb.Collection("notifications");
            var notification = new Dictionary<string, object>
            {
                { "receiverId", message.UserId },
                { "title", "Payment Successful" },
                { "message", $"You have successfully enrolled in the course: {message.CourseId}." },
                { "timestamp", FieldValue.ServerTimestamp },
                { "isRead", false },
                { "type", "PAYMENT_SUCCESS" }
            };

            await notificationRef.AddAsync(notification);
        }
    }
}
