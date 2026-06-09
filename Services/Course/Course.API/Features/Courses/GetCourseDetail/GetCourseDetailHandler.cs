using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Courses.GetCourseDetail
{
    public record GetCourseDetailQuery(string Id) : IQuery<IResult>;

    public class GetCourseDetailQueryHandler : IQueryHandler<GetCourseDetailQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetCourseDetailQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetCourseDetailQuery request, CancellationToken cancellationToken)
        {
            var docRef = _firestoreDb.Collection("Courses").Document(request.Id);
            var docSnap = await docRef.GetSnapshotAsync(cancellationToken);
            if (!docSnap.Exists) return Results.NotFound(new { Message = "Course not found." });

            var courseData = docSnap.ToDictionary();

            var regSnap = await _firestoreDb.Collection("courseRegistrations")
                .WhereEqualTo("courseId", request.Id)
                .WhereIn("status", new[] { "accepted", "active" })
                .GetSnapshotAsync(cancellationToken);
            int actualStudentCount = regSnap.Documents.Count;

            var asmSnap = await docRef.Collection("Assignments").GetSnapshotAsync(cancellationToken);
            int actualAssignmentCount = asmSnap.Documents.Count;

            courseData["StudentCount"] = actualStudentCount;
            courseData["AssignmentCount"] = actualAssignmentCount;

            var lessonsSnap = await _firestoreDb.Collection("Lessons")
                .WhereEqualTo("CourseId", request.Id)
                .GetSnapshotAsync(cancellationToken);
            var lessons = lessonsSnap.Documents
                .Select(d => new { Id = d.Id, Data = CourseHelper.ConvertFirestoreTypes(d.ToDictionary()) })
                .OrderBy(l => l.Data.ContainsKey("CreatedAt") ? l.Data["CreatedAt"] : null)
                .ToList();

            return Results.Ok(new
            {
                Id = docSnap.Id,
                Data = CourseHelper.ConvertFirestoreTypes(courseData),
                Lessons = lessons
            });
        }
    }
}
