using BuildingBlocks.CQRS;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using MediatR;

namespace Identity.API.Features.Auth.Register
{
    public record RegisterCommand(string Email, string Password, string FullName) : ICommand<IResult>;

    public class RegisterCommandHandler : ICommandHandler<RegisterCommand, IResult>
    {
        private readonly FirestoreDb _firestoreDb;

        public RegisterCommandHandler(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userArgs = new UserRecordArgs()
                {
                    Email = request.Email,
                    Password = request.Password,
                    DisplayName = request.FullName,
                };
                UserRecord userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);

                var userDoc = _firestoreDb.Collection("Users").Document(userRecord.Uid);
                var userData = new Dictionary<string, object>
                {
                    { "Email", request.Email },
                    { "FullName", request.FullName },
                    { "Role", "Student" },
                    { "PhoneNumber", "" },
                    { "CreatedAt", DateTime.UtcNow },
                    { "IsBlocked", false },
                    { "ProfileImageUrl", "" }
                };
                await userDoc.SetAsync(userData, cancellationToken: cancellationToken);

                return Results.Ok(new { Message = "Đăng ký thành công", Uid = userRecord.Uid });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Message = "Lỗi đăng ký", Error = ex.Message });
            }
        }
    }
}
