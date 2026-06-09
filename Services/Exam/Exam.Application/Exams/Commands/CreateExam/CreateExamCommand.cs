using BuildingBlocks.CQRS;
using Exam.Application.Dtos;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Exams.Commands.CreateExam
{
    public record CreateExamCommand(string InstructorId, CreateExamRequest Request) : ICommand<IResult>;
}
