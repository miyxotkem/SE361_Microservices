using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Course.API.Features.Courses.GetAllCourses
{
    public record GetAllCoursesQuery() : IQuery<IResult>;

    public class GetAllCoursesQueryHandler : IQueryHandler<GetAllCoursesQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetAllCoursesQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
        {
            var snapshot = await _firestoreDb.Collection("Courses").GetSnapshotAsync(cancellationToken);
            var courses = new List<object>();

            foreach (var doc in snapshot.Documents)
            {
                var courseData = doc.ToDictionary();
                string id = doc.Id;

                var regSnap = await _firestoreDb.Collection("courseRegistrations")
                    .WhereEqualTo("courseId", id)
                    .WhereIn("status", new[] { "accepted", "active" })
                    .GetSnapshotAsync(cancellationToken);
                int actualStudentCount = regSnap.Documents.Count;

                var asmSnapDoc = await _firestoreDb.Collection("Courses").Document(id)
                                                   .Collection("Assignments").GetSnapshotAsync(cancellationToken);
                int actualAssignmentCount = asmSnapDoc.Documents.Count;

                courseData["StudentCount"] = actualStudentCount;
                courseData["AssignmentCount"] = actualAssignmentCount;

                courses.Add(new
                {
                    Id = id,
                    Data = CourseHelper.ConvertFirestoreTypes(courseData)
                });
            }

            return Results.Ok(courses);
        }
    }
}
