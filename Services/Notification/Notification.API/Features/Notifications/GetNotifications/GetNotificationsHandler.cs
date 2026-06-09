using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Notification.API.Features.Notifications.GetNotifications
{
    public record GetNotificationsQuery(string Uid) : IQuery<IResult>;

    public class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetNotificationsQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            // Get active course IDs to hide notifications of deleted courses
            var coursesSnap = await _firestoreDb.Collection("Courses").GetSnapshotAsync(cancellationToken);
            var activeCourseIds = coursesSnap.Documents.Select(d => d.Id).ToHashSet();

            // Personal notifications
            var userNotifsSnap = await _firestoreDb.Collection("Notifications")
                .WhereEqualTo("TargetId", request.Uid)
                .GetSnapshotAsync(cancellationToken);

            // System notifications
            var systemNotifsSnap = await _firestoreDb.Collection("Notifications")
                .WhereEqualTo("TargetId", "all")
                .GetSnapshotAsync(cancellationToken);

            // Read notifications sub-collection for this student
            var readSnap = await _firestoreDb.Collection("Users").Document(request.Uid)
                .Collection("ReadNotifications").GetSnapshotAsync(cancellationToken);
            
            var readIds = readSnap.Documents.Select(d => d.Id).ToHashSet();

            var notifications = new List<object>();

            foreach (var doc in userNotifsSnap.Documents.Concat(systemNotifsSnap.Documents))
            {
                var dict = doc.ToDictionary();
                string courseId = dict.ContainsKey("CourseId") ? dict["CourseId"].ToString() ?? "" : "";
                
                if (!string.IsNullOrEmpty(courseId) && !activeCourseIds.Contains(courseId))
                {
                    continue;
                }

                bool isRead = dict.ContainsKey("IsRead") && Convert.ToBoolean(dict["IsRead"]);
                
                if (dict.ContainsKey("TargetId") && dict["TargetId"].ToString() == "all")
                {
                    isRead = readIds.Contains(doc.Id);
                }

                notifications.Add(new
                {
                    Id = doc.Id,
                    Title = dict.ContainsKey("Title") ? dict["Title"].ToString() ?? "" : "",
                    Content = dict.ContainsKey("Content") ? dict["Content"].ToString() ?? "" : "",
                    SenderId = dict.ContainsKey("SenderId") ? dict["SenderId"].ToString() ?? "" : "",
                    SenderName = dict.ContainsKey("SenderName") ? dict["SenderName"].ToString() ?? "" : "",
                    TargetId = dict.ContainsKey("TargetId") ? dict["TargetId"].ToString() ?? "" : "",
                    CourseId = courseId,
                    Type = dict.ContainsKey("Type") ? dict["Type"].ToString() ?? "" : "",
                    CreatedAt = dict.ContainsKey("CreatedAt") ? ((Timestamp)dict["CreatedAt"]).ToDateTime().ToUniversalTime() : DateTime.UtcNow,
                    IsRead = isRead
                });
            }

            var sorted = notifications.OrderByDescending(n => (DateTime)((dynamic)n).CreatedAt).ToList();
            return Results.Ok(sorted);
        }
    }
}
