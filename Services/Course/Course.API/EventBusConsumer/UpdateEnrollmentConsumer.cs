using BuildingBlocks.Messaging.Commands;
using BuildingBlocks.Messaging.Events;
using Google.Cloud.Firestore;
using MassTransit;

namespace Course.API.EventBusConsumer;

public class UpdateEnrollmentConsumer : IConsumer<UpdateEnrollmentCommand>
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<UpdateEnrollmentConsumer> _logger;

    public UpdateEnrollmentConsumer(FirestoreDb firestoreDb, ILogger<UpdateEnrollmentConsumer> logger)
    {
        _firestoreDb = firestoreDb;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UpdateEnrollmentCommand> context)
    {
        var command = context.Message;
        _logger.LogInformation("Processing UpdateEnrollmentCommand for UserId: {UserId}, CourseId: {CourseId}", command.UserId, command.CourseId);

        try
        {
            var registrationsRef = _firestoreDb.Collection("courseRegistrations");
            var query = registrationsRef
                .WhereEqualTo("userId", command.UserId)
                .WhereEqualTo("courseId", command.CourseId);

            var querySnapshot = await query.GetSnapshotAsync();
            if (querySnapshot.Documents.Count > 0)
            {
                var docRef = querySnapshot.Documents[0].Reference;
                await docRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "status", "active" },
                    { "transactionId", command.TransactionId },
                    { "amount", (double)command.Amount },
                    { "updatedAt", Timestamp.FromDateTime(DateTime.UtcNow) }
                });

                _logger.LogInformation("Successfully updated enrollment status to active.");

                // Publish Success
                await context.Publish(new EnrollmentActivatedEvent
                {
                    CorrelationId = command.CorrelationId,
                    UserId = command.UserId,
                    CourseId = command.CourseId
                });
            }
            else
            {
                throw new Exception("Course registration not found for the user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update enrollment status.");

            // Publish Failure
            await context.Publish(new EnrollmentFailedEvent
            {
                CorrelationId = command.CorrelationId,
                UserId = command.UserId,
                CourseId = command.CourseId,
                Reason = ex.Message
            });
        }
    }
}
