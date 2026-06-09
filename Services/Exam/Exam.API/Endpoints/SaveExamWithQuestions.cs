using Carter;
using Exam.Application.Exams.Commands.SaveExamWithQuestions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Exam.API.Endpoints
{
    public class SaveExamWithQuestions : ICarterModule
    {
        public class SaveExamWithQuestionsRequest
        {
            public string ExamId { get; set; } = string.Empty;
            public string ClassId { get; set; } = string.Empty;
            public string? ClassName { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public int TimeLimitMinutes { get; set; }
            public double PassingScore { get; set; }
            public bool IsPublished { get; set; }
            public bool IsActive { get; set; }
            public bool AllowReview { get; set; }
            public bool RandomizeQuestions { get; set; }
            public bool ShowScore { get; set; }
            public bool AllowMultipleAttempts { get; set; }
            public int MaxAttempts { get; set; }
            public List<SaveExamQuestionDto> Questions { get; set; } = new();
        }

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("api/exams/with-questions", async (SaveExamWithQuestionsRequest request, ClaimsPrincipal user, ISender sender) =>
            {
                string uid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("UID not found in claims");
                var command = new SaveExamWithQuestionsCommand(
                    request.ExamId,
                    request.ClassId,
                    request.ClassName,
                    request.Title,
                    request.Description,
                    request.TimeLimitMinutes,
                    request.PassingScore,
                    request.IsPublished,
                    request.IsActive,
                    request.AllowReview,
                    request.RandomizeQuestions,
                    request.ShowScore,
                    request.AllowMultipleAttempts,
                    request.MaxAttempts,
                    request.Questions,
                    uid
                );
                return await sender.Send(command);
            }).RequireAuthorization(policy => policy.RequireRole("Instructor", "Admin"));
        }
    }
}
