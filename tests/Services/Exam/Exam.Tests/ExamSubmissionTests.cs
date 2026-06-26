using System;
using System.Collections.Generic;
using Exam.Domain.Entities;
using Exam.Domain.Models;
using FluentAssertions;
using Xunit;

namespace Exam.Tests;

public class ExamSubmissionTests
{
    private readonly Exam.Domain.Models.Exam _exam;
    private readonly Question _question1;
    private readonly Question _question2;

    public ExamSubmissionTests()
    {
        _exam = new Exam.Domain.Models.Exam
        {
            Id = "exam-123",
            ClassId = "class-456",
            Title = "Math Exam",
            IsPublished = false
        };

        _question1 = new Question("q1", "1 + 1 = ?", new List<string> { "1", "2", "3" }, 1, 4.0); // 4 points
        _question2 = new Question("q2", "2 * 2 = ?", new List<string> { "2", "4", "6" }, 1, 6.0); // 6 points
    }

    [Fact]
    public void AddQuestion_ShouldAddSuccessfully_WhenExamIsNotPublished()
    {
        // Act
        _exam.AddQuestion(_question1);

        // Assert
        _exam.Questions.Should().ContainSingle(q => q.QuestionId == "q1");
        _exam.QuestionIds.Should().Contain("q1");
        _exam.TotalQuestions.Should().Be(1);
    }

    [Fact]
    public void AddQuestion_ShouldThrowInvalidOperationException_WhenExamIsPublished()
    {
        // Arrange
        _exam.AddQuestion(_question1);
        _exam.Publish();

        // Act
        Action act = () => _exam.AddQuestion(_question2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot add question to a published exam*");
    }

    [Fact]
    public void Publish_ShouldThrowInvalidOperationException_WhenExamHasNoQuestions()
    {
        // Act
        Action act = () => _exam.Publish();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot publish an exam without questions*");
    }

    [Fact]
    public void Grade_ShouldCalculatePerfectScore_WhenAllAnswersAreCorrect()
    {
        // Arrange
        _exam.AddQuestion(_question1);
        _exam.AddQuestion(_question2);

        var submission = new ExamSubmission();
        var studentAnswers = new Dictionary<string, int>
        {
            { "0", 1 }, // Index of correct answer "2" for question1
            { "1", 1 }  // Index of correct answer "4" for question2
        };

        // Act
        submission.Grade(_exam, studentAnswers, "Alice");

        // Assert
        submission.StudentName.Should().Be("Alice");
        submission.ExamId.Should().Be("exam-123");
        submission.CourseId.Should().Be("class-456");
        submission.TotalQuestions.Should().Be(2);
        submission.Percentage.Should().Be(100.0);
        submission.Score.Should().Be(10.0); // Perfect score out of 10.0 scale

        submission.Answers.Should().HaveCount(2);
        submission.Answers[0].IsCorrect.Should().BeTrue();
        submission.Answers[0].PointsEarned.Should().Be(4.0);
        submission.Answers[1].IsCorrect.Should().BeTrue();
        submission.Answers[1].PointsEarned.Should().Be(6.0);
    }

    [Fact]
    public void Grade_ShouldCalculatePartialScore_WhenSomeAnswersAreCorrect()
    {
        // Arrange
        _exam.AddQuestion(_question1);
        _exam.AddQuestion(_question2);

        var submission = new ExamSubmission();
        var studentAnswers = new Dictionary<string, int>
        {
            { "0", 1 }, // Correct (4.0 points)
            { "1", 2 }  // Incorrect (0 points)
        };

        // Act
        submission.Grade(_exam, studentAnswers, "Bob");

        // Assert
        submission.StudentName.Should().Be("Bob");
        // Total points = 10.0. Earned points = 4.0.
        // Percentage = (4.0 / 10.0) * 100 = 40.0%
        // Score = 40.0% / 10.0 = 4.0
        submission.Percentage.Should().Be(40.0);
        submission.Score.Should().Be(4.0);

        submission.Answers[0].IsCorrect.Should().BeTrue();
        submission.Answers[0].PointsEarned.Should().Be(4.0);
        submission.Answers[1].IsCorrect.Should().BeFalse();
        submission.Answers[1].PointsEarned.Should().Be(0.0);
    }

    [Fact]
    public void Grade_ShouldCalculateZeroScore_WhenAllAnswersAreIncorrectOrMissing()
    {
        // Arrange
        _exam.AddQuestion(_question1);
        _exam.AddQuestion(_question2);

        var submission = new ExamSubmission();
        var studentAnswers = new Dictionary<string, int>
        {
            { "0", 2 }, // Incorrect
            // "1" is missing
        };

        // Act
        submission.Grade(_exam, studentAnswers, "Charlie");

        // Assert
        submission.Percentage.Should().Be(0.0);
        submission.Score.Should().Be(0.0);

        submission.Answers[0].IsCorrect.Should().BeFalse();
        submission.Answers[1].IsCorrect.Should().BeFalse();
    }
}
