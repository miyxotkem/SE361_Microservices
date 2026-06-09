using BuildingBlocks.CQRS;
using MediatR;
using System.Text;
using System.Text.Json;

namespace Identity.API.Features.Auth.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : ICommand<IResult>;

    public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, IResult>
    {
        private readonly IConfiguration _config;

        public ForgotPasswordCommandHandler(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var apiKey = _config["Firebase:ApiKey"];
                    var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}";
                    var payload = new
                    {
                        requestType = "PASSWORD_RESET",
                        email = request.Email
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(url, content, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                        return Results.BadRequest(new { Message = "Lỗi gửi yêu cầu khôi phục mật khẩu", Error = errorResponse });
                    }

                    return Results.Ok(new { Message = "Đã gửi email khôi phục mật khẩu!" });
                }
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Message = "Lỗi yêu cầu reset password", Error = ex.Message });
            }
        }
    }
}
