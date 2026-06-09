using Carter;
using MediatR;

namespace Course.API.Features.Registrations.RemoveRegistration
{
    public class RemoveRegistrationEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/courses/{courseId}/registrations/{regId}", async (string courseId, string regId, ISender sender) =>
            {
                var cmd = new RemoveRegistrationCommand(courseId, regId);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"));
        }
    }
}
