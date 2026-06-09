using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using System.Text.Json;

namespace Course.API.Features.Courses.UpdateCourse
{
    public record UpdateCourseCommand(string Id, JsonElement RequestBody) : ICommand<IResult>;

    public class UpdateCourseCommandHandler : ICommandHandler<UpdateCourseCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public UpdateCourseCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            var docRef = _firestoreDb.Collection("Courses").Document(request.Id);
            var docSnap = await docRef.GetSnapshotAsync(cancellationToken);
            if (!docSnap.Exists) return Results.NotFound(new { Message = "Course not found." });

            var allowedFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Title", "Title" }, { "Description", "Description" }, { "ClassName", "ClassName" },
                { "Category", "Category" }, { "Semester", "Semester" }, { "Emoji", "Emoji" },
                { "AccentColor", "AccentColor" }, { "StudentCount", "StudentCount" },
                { "AssignmentCount", "AssignmentCount" }, { "IsActive", "IsActive" },
                { "DayOfWeek", "DayOfWeek" }, { "StartPeriod", "StartPeriod" }, { "EndPeriod", "EndPeriod" },
                { "Price", "Price" }, { "ThumbnailUrl", "ThumbnailUrl" }, { "InstructorId", "InstructorId" },
                { "CourseType", "CourseType" }
            };

            var updates = new Dictionary<string, object>();
            foreach (var property in request.RequestBody.EnumerateObject())
            {
                if (allowedFields.TryGetValue(property.Name, out string? firestoreKey))
                {
                    var val = property.Value;
                    switch (val.ValueKind)
                    {
                        case JsonValueKind.Null:
                            updates[firestoreKey] = null!;
                            break;
                        case JsonValueKind.String:
                            updates[firestoreKey] = val.GetString() ?? "";
                            break;
                        case JsonValueKind.Number:
                            if (val.TryGetInt32(out int iVal)) updates[firestoreKey] = iVal;
                            else if (val.TryGetDouble(out double dVal)) updates[firestoreKey] = dVal;
                            break;
                        case JsonValueKind.True:
                            updates[firestoreKey] = true;
                            break;
                        case JsonValueKind.False:
                            updates[firestoreKey] = false;
                            break;
                        default:
                            updates[firestoreKey] = val.ToString();
                            break;
                    }
                }
            }

            if (updates.Count > 0)
            {
                updates["UpdatedAt"] = DateTime.UtcNow;
                await docRef.UpdateAsync(updates, cancellationToken: cancellationToken);
            }

            return Results.Ok(new { Message = "Course updated successfully." });
        }
    }
}
