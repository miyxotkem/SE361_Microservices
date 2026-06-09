using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Exam.Application.Exams.Commands.SubmitExam
{
    public record SubmitExamCommand(string ExamId, string StudentId, Dictionary<string, int> Answers, int TimeSpentSeconds) : ICommand<IResult>;
}
