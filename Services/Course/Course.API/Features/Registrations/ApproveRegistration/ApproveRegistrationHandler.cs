using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events;
using Google.Cloud.Firestore;
using MassTransit;
using MediatR;

namespace Course.API.Features.Registrations.ApproveRegistration
{
    public record ApproveRegistrationCommand(string CourseId, string RegId) : ICommand<IResult>;

    public class ApproveRegistrationCommandHandler : ICommandHandler<ApproveRegistrationCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly IPublishEndpoint _publishEndpoint;

        public ApproveRegistrationCommandHandler(FirestoreDb firestoreDb, IPublishEndpoint publishEndpoint)
        {
            _firestoreDb = firestoreDb;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<IResult> Handle(ApproveRegistrationCommand request, CancellationToken cancellationToken)
        {
            var regRef = _firestoreDb.Collection("courseRegistrations").Document(request.RegId);
            var regSnapshot = await regRef.GetSnapshotAsync(cancellationToken);
            if (!regSnapshot.Exists) return Results.NotFound("Registration not found.");

            var courseRef = _firestoreDb.Collection("Courses").Document(request.CourseId);
            var courseSnapshot = await courseRef.GetSnapshotAsync(cancellationToken);
            if (!courseSnapshot.Exists) return Results.NotFound("Course not found.");

            // Get course price. Default to 0 if not found.
            var price = courseSnapshot.TryGetValue<double>("Price", out var p) ? p : 0;
            var userId = regSnapshot.GetValue<string>("userId");

            // If course is free (0 VND), activate immediately
            string newStatus = price == 0 ? "active" : "accepted";

            var updates = new Dictionary<string, object>
            {
                { "status", newStatus },
                { "approvedDate", FieldValue.ServerTimestamp }
            };
            await regRef.UpdateAsync(updates, cancellationToken: cancellationToken);

            // Publish event if activated immediately (free course)
            if (newStatus == "active")
            {
                await _publishEndpoint.Publish(new EnrollmentActivatedEvent
                {
                    CorrelationId = Guid.NewGuid(), // Generate new for free courses
                    UserId = userId,
                    CourseId = request.CourseId
                }, cancellationToken);
            }

            return Results.Ok(new { Message = "Registration approved.", Status = newStatus });
        }
    }
}
