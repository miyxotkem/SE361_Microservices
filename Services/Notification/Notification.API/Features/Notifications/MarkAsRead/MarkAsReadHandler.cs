using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Notification.API.Features.Notifications.MarkAsRead
{
    public record MarkAsReadCommand(string Uid, List<string> NotificationIds) : ICommand<IResult>;

    public class MarkAsReadCommandHandler : ICommandHandler<MarkAsReadCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public MarkAsReadCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
        {
            foreach (var id in request.NotificationIds)
            {
                var notifRef = _firestoreDb.Collection("Notifications").Document(id);
                var snap = await notifRef.GetSnapshotAsync(cancellationToken);
                if (snap.Exists)
                {
                    var dict = snap.ToDictionary();
                    if (dict.ContainsKey("TargetId") && dict["TargetId"].ToString() == "all")
                    {
                        await _firestoreDb.Collection("Users").Document(request.Uid)
                            .Collection("ReadNotifications").Document(id)
                            .SetAsync(new { ReadAt = DateTime.UtcNow }, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await notifRef.UpdateAsync("IsRead", true, cancellationToken: cancellationToken);
                    }
                }
            }

            return Results.Ok(new { Message = "Notifications marked as read" });
        }
    }
}
