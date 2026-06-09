using Google.Cloud.Firestore;
using Grpc.Core;
using Identity.API.Grpc;

namespace Identity.API.Services
{
    public class UserGrpcService : UserProtoService.UserProtoServiceBase
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<UserGrpcService> _logger;

        public UserGrpcService(FirestoreDb firestoreDb, ILogger<UserGrpcService> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        public override async Task<UserProfileModel> GetUserProfile(GetUserProfileRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC GetUserProfile called for UserId: {UserId}", request.UserId);

            var docRef = _firestoreDb.Collection("Users").Document(request.UserId);
            var snap = await docRef.GetSnapshotAsync(context.CancellationToken);

            if (!snap.Exists)
            {
                _logger.LogWarning("gRPC GetUserProfile - UserId {UserId} not found in Firestore.", request.UserId);
                throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found."));
            }

            var dict = snap.ToDictionary();
            string fullName = dict.ContainsKey("FullName") ? dict["FullName"].ToString() ?? "Student" : "Student";
            string email = dict.ContainsKey("Email") ? dict["Email"].ToString() ?? "" : "";
            string role = dict.ContainsKey("Role") ? dict["Role"].ToString() ?? "Student" : "Student";

            return new UserProfileModel
            {
                UserId = request.UserId,
                FullName = fullName,
                Email = email,
                Role = role
            };
        }
    }
}
