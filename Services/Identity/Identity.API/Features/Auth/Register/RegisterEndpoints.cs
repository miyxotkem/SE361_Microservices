using Carter;
using MediatR;

namespace Identity.API.Features.Auth.Register
{
    public class RegisterEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/auth/register", async (RegisterRequest req, ISender sender) =>
            {
                var cmd = new RegisterCommand(req.Email, req.Password, req.FullName);
                return await sender.Send(cmd);
            });
        }

        public class RegisterRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
        }
    }
}
