using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using System.Text;
using System.Text.Json;

namespace Identity.API.Features.Auth.LoginGoogle
{
    public record LoginGoogleCommand(string FirebaseToken) : ICommand<IResult>;

    public class LoginGoogleCommandHandler : ICommandHandler<LoginGoogleCommand, IResult>
    {
        private readonly IConfiguration _config;
        private readonly FirestoreDb _firestoreDb;

        public LoginGoogleCommandHandler(IConfiguration config, FirestoreDb firestoreDb)
        {
            _config = config;
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(LoginGoogleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var apiKey = _config["Firebase:ApiKey"];
                    var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key={apiKey}";
                    var payload = new
                    {
                        postBody = $"id_token={request.FirebaseToken}&providerId=google.com",
                        requestUri = "http://localhost",
                        returnIdToken = true,
                        returnSecureToken = true
                    };
                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(url, content, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errStr = await response.Content.ReadAsStringAsync(cancellationToken);
                        return Results.Json(new { Message = "Xác thực với Google Identity Toolkit thất bại.", Error = errStr }, statusCode: StatusCodes.Status401Unauthorized);
                    }

                    var resStr = await response.Content.ReadAsStringAsync(cancellationToken);
                    using var doc = JsonDocument.Parse(resStr);
                    var root = doc.RootElement;

                    string uid = root.GetProperty("localId").GetString() ?? "";
                    string email = root.GetProperty("email").GetString() ?? "";
                    string displayName = root.TryGetProperty("displayName", out var dp) ? (dp.GetString() ?? email) : email;
                    string photoUrl = root.TryGetProperty("photoUrl", out var pu) ? (pu.GetString() ?? "") : "";

                    string role = "Instructor"; // Default to Instructor so user can create courses
                    bool isBlocked = false;

                    try
                    {
                        var userDocRef = _firestoreDb.Collection("Users").Document(uid);
                        var userDoc = await userDocRef.GetSnapshotAsync(cancellationToken);

                        if (!userDoc.Exists)
                        {
                            var userData = new Dictionary<string, object>
                            {
                                { "Email", email },
                                { "FullName", displayName },
                                { "Role", "Instructor" },
                                { "PhoneNumber", "" },
                                { "CreatedAt", DateTime.UtcNow },
                                { "IsBlocked", false },
                                { "ProfileImageUrl", photoUrl }
                            };
                            await userDocRef.SetAsync(userData, cancellationToken: cancellationToken);
                        }
                        else
                        {
                            if (userDoc.TryGetValue("Role", out string dbRole))
                            {
                                role = dbRole;
                            }
                            if (userDoc.TryGetValue("IsBlocked", out bool dbBlocked))
                            {
                                isBlocked = dbBlocked;
                            }
                        }
                        
                        if (isBlocked)
                        {
                            return Results.BadRequest(new { Message = "Tài khoản của bạn đã bị khóa bởi Admin!" });
                        }
                    }
                    catch (Exception fex)
                    {
                        Console.WriteLine($"[LoginGoogleHandler] Firestore error bypassed: {fex.Message}");
                        // Bỏ qua lỗi Quota Exceeded và cho phép đăng nhập thành công
                    }

                    var jwtToReturn = AuthHelper.GenerateJwtToken(uid, email, role, _config);

                    return Results.Ok(new
                    {
                        Token = jwtToReturn,
                        Uid = uid,
                        Email = email,
                        DisplayName = displayName,
                        FirebaseToken = root.GetProperty("idToken").GetString(),
                        ExpiresIn = int.TryParse(root.GetProperty("expiresIn").GetString(), out int exp) ? exp : 3600
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginGoogleHandler] Exception: {ex}");
                return Results.Json(new { Message = "Token không hợp lệ", Error = ex.Message }, statusCode: StatusCodes.Status401Unauthorized);
            }
        }
    }
}
