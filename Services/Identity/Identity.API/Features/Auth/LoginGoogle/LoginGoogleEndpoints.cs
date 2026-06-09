using Carter;
using MediatR;

namespace Identity.API.Features.Auth.LoginGoogle
{
    public class LoginGoogleEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/auth/Login-google", async (LoginGoogleRequest req, ISender sender) =>
            {
                var cmd = new LoginGoogleCommand(req.FirebaseToken);
                return await sender.Send(cmd);
            });
        }

        public class LoginGoogleRequest
        {
            public string FirebaseToken { get; set; } = string.Empty;
        }
    }
}
