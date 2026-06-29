using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Course.API.Features.Courses.GetAllCourses
{
    public record GetAllCoursesQuery() : IQuery<IResult>;

    public class GetAllCoursesQueryHandler : IQueryHandler<GetAllCoursesQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly IDistributedCache _cache;

        public GetAllCoursesQueryHandler(FirestoreDb firestoreDb, IDistributedCache cache)
        {
            _firestoreDb = firestoreDb;
            _cache = cache;
        }

        public async Task<IResult> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = "all_courses";
            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return Results.Content(cachedData, "application/json");
            }

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

            var jsonResult = JsonSerializer.Serialize(courses);
            await _cache.SetStringAsync(cacheKey, jsonResult, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            }, cancellationToken);

            return Results.Content(jsonResult, "application/json");
        }
    }
}
