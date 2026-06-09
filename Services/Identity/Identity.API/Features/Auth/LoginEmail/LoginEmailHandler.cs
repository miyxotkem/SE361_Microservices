using BuildingBlocks.CQRS;
using Google.Cloud.Firestore;
using MediatR;
using System.Text;
using System.Text.Json;

namespace Identity.API.Features.Auth.LoginEmail
{
    public record LoginEmailCommand(string Email, string Password) : ICommand<IResult>;

    public class GoogleAuthResult
    {
        public string LocalId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string ExpiresIn { get; set; } = string.Empty;
    }

    public class LoginEmailCommandHandler : ICommandHandler<LoginEmailCommand, IResult>
    {
        private readonly IConfiguration _config;
        private readonly FirestoreDb _firestoreDb;

        public LoginEmailCommandHandler(IConfiguration config, FirestoreDb firestoreDb)
        {
            _config = config;
            _firestoreDb = firestoreDb;
        }

        public async Task<IResult> Handle(LoginEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // -- Special Admin --
                if (request.Email == "admin" && request.Password == "admin")
                {
                    string uidAdmin = "admin_super_id";
                    string roleAdmin = "Admin";
                    var tokenAdmin = AuthHelper.GenerateJwtToken(uidAdmin, "admin@system.com", roleAdmin, _config);
                    return Results.Ok(new
                    {
                        Token = tokenAdmin,
                        Uid = uidAdmin,
                        Email = "admin@system.com",
                        DisplayName = "Administrator",
                        FirebaseToken = "fake-token",
                        ExpiresIn = 86400
                    });
                }

                using (var httpClient = new HttpClient())
                {
                    var apiKey = _config["Firebase:ApiKey"];
                    var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";
                    var payload = new
                    {
                        email = request.Email,
                        password = request.Password,
                        returnSecureToken = true
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(url, content, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                        string errorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                        try
                        {
                            var errDoc = JsonDocument.Parse(errorResponse).RootElement;
                            if (errDoc.TryGetProperty("error", out var errProp) && errProp.TryGetProperty("message", out var msgProp))
                            {
                                string firebaseError = msgProp.GetString() ?? "";
                                if (firebaseError == "EMAIL_NOT_FOUND" || firebaseError == "INVALID_PASSWORD" || firebaseError == "INVALID_LOGIN_CREDENTIALS")
                                {
                                    errorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                                }
                                else if (firebaseError == "USER_DISABLED")
                                {
                                    errorMessage = "Tài khoản này đã bị vô hiệu hóa.";
                                }
                                else
                                {
                                    errorMessage = firebaseError;
                                }
                            }
                        }
                        catch { }
                        return Results.Json(new { Message = errorMessage }, statusCode: StatusCodes.Status401Unauthorized);
                    }

                    var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                    var googleAuthResult = JsonSerializer.Deserialize<GoogleAuthResult>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (googleAuthResult == null || string.IsNullOrEmpty(googleAuthResult.LocalId))
                    {
                        return Results.Json(new { Message = "Không thể xác thực thông tin đăng nhập." }, statusCode: StatusCodes.Status401Unauthorized);
                    }

                    var uid = googleAuthResult.LocalId;
                    var userDoc = await _firestoreDb.Collection("Users").Document(uid).GetSnapshotAsync(cancellationToken);
                    string role = "Student";
                    bool isBlocked = false;
                    if (userDoc.Exists)
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

                    var jwtToReturn = AuthHelper.GenerateJwtToken(uid, googleAuthResult.Email, role, _config);

                    return Results.Ok(new
                    {
                        Token = jwtToReturn,
                        Uid = uid,
                        Email = googleAuthResult.Email,
                        DisplayName = googleAuthResult.DisplayName,
                        FirebaseToken = googleAuthResult.IdToken,
                        ExpiresIn = int.TryParse(googleAuthResult.ExpiresIn, out int exp) ? exp : 3600
                    });
                }
            }
            catch (Exception ex)
            {
                return Results.Json(new { Message = "Lỗi xử lý đăng nhập hệ thống", Error = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
