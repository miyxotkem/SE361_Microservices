using Carter;
using MediatR;

namespace Identity.API.Features.Auth.ForgotPassword
{
    public class ForgotPasswordEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/auth/forgot-password", async (ForgotPasswordRequest req, ISender sender) =>
            {
                var cmd = new ForgotPasswordCommand(req.Email);
                return await sender.Send(cmd);
            });
        }

        public class ForgotPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
        }
    }
}
