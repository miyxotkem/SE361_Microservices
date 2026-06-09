using BuildingBlocks.CQRS;
using Exam.Application.Data;
using Exam.Application.Services;
using Exam.Domain.Entities;
using Exam.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Exam.Application.Exams.Commands.SubmitExam
{
    public class SubmitExamCommandHandler : ICommandHandler<SubmitExamCommand, IResult>
    {
        private readonly IExamRepository _examRepository;
        private readonly IUserServiceClient _userServiceClient;

        public SubmitExamCommandHandler(IExamRepository examRepository, IUserServiceClient userServiceClient)
        {
            _examRepository = examRepository;
            _userServiceClient = userServiceClient;
        }

        public async Task<IResult> Handle(SubmitExamCommand request, CancellationToken cancellationToken)
        {
            var exam = await _examRepository.GetByIdAsync(request.ExamId);
            if (exam == null) return Results.NotFound(new { Message = "Exam not found." });

            string studentName = "Student";
            try
            {
                // gRPC Call: Get student name
                studentName = await _userServiceClient.GetUserFullNameAsync(request.StudentId);
            }
            catch { }

            // Domain Logic: Create Submission & Grade
            var submission = new ExamSubmission();
            submission.Grade(exam, request.Answers, studentName);
            submission.StudentId = request.StudentId;
            submission.TimeSpentSeconds = request.TimeSpentSeconds;

            await _examRepository.CreateSubmissionAsync(submission);

            // Return immediate results as monolith did
            int correctCount = submission.Answers.Count(a => a.IsCorrect);
            return Results.Ok(new
            {
                Message = "Exam submitted successfully.",
                Score = correctCount,
                TotalQuestions = submission.TotalQuestions,
                Percentage = submission.Percentage,
                StudentName = studentName,
                SubmittedAt = submission.SubmittedAt
            });
        }
    }
}
