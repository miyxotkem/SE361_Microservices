using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;

namespace Identity.API.Features.Users.GetAllUsers
{
    public record GetAllUsersQuery() : IQuery<IResult>;

    public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public GetAllUsersQueryHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("Users").GetSnapshotAsync(cancellationToken);
                var users = snapshot.Documents.Select(d => new
                {
                    Id = d.Id,
                    Data = UserHelper.ConvertFirestoreTypes(d.ToDictionary())
                });

                return Results.Ok(users);
            }
            catch (Exception ex)
            {
                return Results.Json(new { Message = "Lỗi khi lấy danh sách user từ Backend", Error = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
