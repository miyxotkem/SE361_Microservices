using Carter;
using MediatR;

namespace Course.API.Features.Registrations.ApproveRegistration
{
    public class ApproveRegistrationEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/courses/{courseId}/registrations/{regId}/approve", async (string courseId, string regId, ISender sender) =>
            {
                var cmd = new ApproveRegistrationCommand(courseId, regId);
                return await sender.Send(cmd);
            }).RequireAuthorization(policy => policy.RequireRole("Admin", "Instructor"));
        }
    }
}
