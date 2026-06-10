using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events;
using Exam.Application.Data;
using Exam.Application.Services;
using Exam.Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Commands.CreateExam
{
    public class CreateExamCommandHandler : ICommandHandler<CreateExamCommand, IResult>
    {
        private readonly IExamRepository _examRepository;
        private readonly ICourseServiceClient _courseServiceClient;
        private readonly IUserServiceClient _userServiceClient;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateExamCommandHandler(
            IExamRepository examRepository,
            ICourseServiceClient courseServiceClient,
            IUserServiceClient userServiceClient,
            IPublishEndpoint publishEndpoint)
        {
            _examRepository = examRepository;
            _courseServiceClient = courseServiceClient;
            _userServiceClient = userServiceClient;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<IResult> Handle(CreateExamCommand request, CancellationToken cancellationToken)
        {
            var req = request.Request;
            string className = "";
            try
            {
                // gRPC Call: Fetch Class Name from Course Service
                className = await _courseServiceClient.GetCourseClassNameAsync(req.CourseId);
            }
            catch { }

            var exam = new Exam.Domain.Models.Exam
            {
                ClassId = req.CourseId,
                ClassName = className,
                Title = req.Title,
                DurationMinutes = req.DurationMinutes,
                TimeLimitMinutes = req.DurationMinutes,
                Description = req.Description,
                PassingScore = req.PassingScore,
                IsPublished = false,
                IsActive = req.IsActive,
                AllowReview = req.AllowReview,
                RandomizeQuestions = req.RandomizeQuestions,
                ShowScore = req.ShowScore,
                AllowMultipleAttempts = req.AllowMultipleAttempts,
                MaxAttempts = req.MaxAttempts,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                InstructorId = request.InstructorId
            };

            foreach (var q in req.Questions)
            {
                var questionId = Guid.NewGuid().ToString("N");
                var questionEntity = new Question(
                    questionId,
                    q.QuestionText,
                    q.Options,
                    q.CorrectOptionIndex,
                    q.Points
                );
                exam.AddQuestion(questionEntity);
            }

            if (req.IsPublished)
            {
                exam.Publish();
            }

            string examId = await _examRepository.CreateAsync(exam);

            // Integration Event: Publish to RabbitMQ if Published
            if (exam.IsPublished)
            {
                string senderName = "Giảng viên";
                try
                {
                    // gRPC Call: Fetch teacher name
                    senderName = await _userServiceClient.GetUserFullNameAsync(request.InstructorId);
                }
                catch { }

                var publishedEvent = new ExamPublishedEvent
                {
                    ExamId = examId,
                    CourseId = exam.ClassId,
                    Title = exam.Title,
                    SenderId = request.InstructorId,
                    SenderName = senderName
                };

                await _publishEndpoint.Publish(publishedEvent, cancellationToken);
            }

            return Results.Ok(new { Message = "Exam created successfully.", Id = examId });
        }
    }
}
