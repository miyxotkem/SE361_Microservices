using Carter;
using MediatR;

namespace Identity.API.Features.Auth.LoginEmail
{
    public class LoginEmailEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/auth/login", async (LoginEmailRequest req, ISender sender) =>
            {
                var cmd = new LoginEmailCommand(req.Email, req.Password);
                return await sender.Send(cmd);
            });
        }

        public class LoginEmailRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
