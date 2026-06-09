using Carter;
using MediatR;

namespace Identity.API.Features.Users.GetAllUsers
{
    public class GetAllUsersEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/users", async (ISender sender) =>
            {
                var query = new GetAllUsersQuery();
                return await sender.Send(query);
            }).RequireAuthorization(policy => policy.RequireRole("Admin"));
        }
    }
}
