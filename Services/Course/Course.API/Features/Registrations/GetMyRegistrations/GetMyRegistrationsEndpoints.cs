using Carter;
using MediatR;
using System.Security.Claims;

namespace Course.API.Features.Registrations.GetMyRegistrations
{
    public class GetMyRegistrationsEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/courses/my-registrations", async (ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in token");
                var query = new GetMyRegistrationsQuery(uid);
                return await sender.Send(query);
            }).RequireAuthorization();
        }
    }
}
