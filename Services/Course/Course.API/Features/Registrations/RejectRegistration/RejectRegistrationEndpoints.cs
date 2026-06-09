using Carter;
using MediatR;

namespace Course.API.Features.Registrations.RejectRegistration
{
    public class RejectRegistrationEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/courses/{courseId}/registrations/{regId}/reject", async (string courseId, string regId, ISender sender) =>
            {
                var cmd = new RejectRegistrationCommand(courseId, regId);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"));
        }
    }
}
