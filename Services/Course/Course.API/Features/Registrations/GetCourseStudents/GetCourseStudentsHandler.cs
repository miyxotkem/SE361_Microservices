using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using Course.API.Features.Courses;

namespace Course.API.Features.Registrations.GetCourseStudents
{
    public record GetCourseStudentsQuery(string CourseId) : IQuery<IResult>;

    public class GetCourseStudentsQueryHandler : IQueryHandler<GetCourseStudentsQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetCourseStudentsQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetCourseStudentsQuery request, CancellationToken cancellationToken)
        {
            var snapshot = await _firestoreDb.Collection("courseRegistrations")
                .WhereEqualTo("courseId", request.CourseId)
                .GetSnapshotAsync(cancellationToken);

            var results = new List<object>();
            foreach (var doc in snapshot.Documents)
            {
                var data = CourseHelper.ConvertFirestoreTypes(doc.ToDictionary()) as Dictionary<string, object> ?? new Dictionary<string, object>();
                string userId = data.ContainsKey("userId") ? data["userId"]?.ToString() ?? "" : "";

                string fullName = "Học viên";
                string email = "";
                if (!string.IsNullOrEmpty(userId))
                {
                    var userDoc = await _firestoreDb.Collection("Users").Document(userId).GetSnapshotAsync(cancellationToken);
                    if (userDoc.Exists)
                    {
                        if (userDoc.TryGetValue("FullName", out string? fn) || userDoc.TryGetValue("fullName", out fn)) fullName = fn;
                        if (userDoc.TryGetValue("Email", out string? em) || userDoc.TryGetValue("email", out em)) email = em;
                    }
                }
                data["fullName"] = fullName;
                data["email"] = email;

                results.Add(new { Id = doc.Id, Data = data });
            }

            return Results.Ok(results);
        }
    }
}
