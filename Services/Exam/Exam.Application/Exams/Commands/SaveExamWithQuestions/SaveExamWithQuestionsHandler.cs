using BuildingBlocks.CQRS;
using Exam.Application.Data;
using Exam.Application.Services;
using Exam.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exam.Application.Exams.Commands.SaveExamWithQuestions
{
    public class SaveExamWithQuestionsCommandHandler : ICommandHandler<SaveExamWithQuestionsCommand, IResult>
    {
        private readonly IExamRepository _examRepository;
        private readonly ICourseServiceClient _courseServiceClient;

        public SaveExamWithQuestionsCommandHandler(IExamRepository examRepository, ICourseServiceClient courseServiceClient)
        {
            _examRepository = examRepository;
            _courseServiceClient = courseServiceClient;
        }

        public async Task<IResult> Handle(SaveExamWithQuestionsCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.ExamId))
            {
                return Results.BadRequest(new { Message = "ExamId is required." });
            }

            string className = request.ClassName ?? "";
            if (string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(request.ClassId))
            {
                className = await _courseServiceClient.GetCourseClassNameAsync(request.ClassId);
            }

            // Map Questions DTOs to Domain Entities
            var questionsList = new List<Question>();
            var questionIds = new List<string>();
            foreach (var q in request.Questions)
            {
                string qId = string.IsNullOrEmpty(q.QuestionId) ? Guid.NewGuid().ToString("N") : q.QuestionId;
                questionIds.Add(qId);
                questionsList.Add(new Question(qId, q.QuestionText, q.Options, q.CorrectOptionIndex, q.Points));
            }

            var existing = await _examRepository.GetByIdAsync(request.ExamId);
            if (existing != null)
            {
                // Update existing exam
                var updates = new Dictionary<string, object>
                {
                    { "ClassId", request.ClassId },
                    { "ClassName", className },
                    { "Title", request.Title ?? "" },
                    { "DurationMinutes", request.TimeLimitMinutes },
                    { "TimeLimitMinutes", request.TimeLimitMinutes },
                    { "Description", request.Description ?? "" },
                    { "PassingScore", request.PassingScore },
                    { "IsPublished", request.IsPublished },
                    { "IsActive", request.IsActive },
                    { "AllowReview", request.AllowReview },
                    { "RandomizeQuestions", request.RandomizeQuestions },
                    { "ShowScore", request.ShowScore },
                    { "AllowMultipleAttempts", request.AllowMultipleAttempts },
                    { "MaxAttempts", request.MaxAttempts },
                    { "TotalQuestions", questionsList.Count },
                    { "QuestionIds", questionIds }
                };

                // Map Questions back to raw dictionaries for storage format
                var rawQuestions = questionsList.Select(q => new Dictionary<string, object>
                {
                    { "QuestionId", q.QuestionId },
                    { "QuestionText", q.QuestionText },
                    { "Options", q.Options },
                    { "CorrectOptionIndex", q.CorrectOptionIndex },
                    { "Points", q.Points }
                }).ToList();
                updates.Add("Questions", rawQuestions);

                await _examRepository.UpdateAsync(request.ExamId, updates);
            }
            else
            {
                // Create new exam
                var exam = new Exam.Domain.Models.Exam
                {
                    Id = request.ExamId,
                    ClassId = request.ClassId,
                    ClassName = className,
                    Title = request.Title ?? "",
                    DurationMinutes = request.TimeLimitMinutes,
                    TimeLimitMinutes = request.TimeLimitMinutes,
                    Description = request.Description ?? "",
                    PassingScore = request.PassingScore,
                    IsPublished = request.IsPublished,
                    IsActive = request.IsActive,
                    AllowReview = request.AllowReview,
                    RandomizeQuestions = request.RandomizeQuestions,
                    ShowScore = request.ShowScore,
                    AllowMultipleAttempts = request.AllowMultipleAttempts,
                    MaxAttempts = request.MaxAttempts,
                    TotalQuestions = questionsList.Count,
                    Questions = questionsList,
                    QuestionIds = questionIds,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    InstructorId = request.CurrentUserId
                };

                await _examRepository.CreateAsync(exam);
            }

            return Results.Ok(new { success = true, Message = "Exam and questions saved successfully." });
        }
    }
}
