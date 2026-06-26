using System;
using System.Collections.Generic;
using Exam.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Exam.Tests;

public class QuestionTests
{
    [Fact]
    public void Constructor_GivenValidParameters_ShouldCreateQuestion()
    {
        // Arrange
        var qId = "q-123";
        var text = "What is the capital of France?";
        var options = new List<string> { "Paris", "London", "Berlin", "Madrid" };
        var correctIndex = 0;
        var points = 1.5;

        // Act
        var question = new Question(qId, text, options, correctIndex, points);

        // Assert
        question.Should().NotBeNull();
        question.QuestionId.Should().Be(qId);
        question.QuestionText.Should().Be(text);
        question.Options.Should().BeEquivalentTo(options);
        question.CorrectOptionIndex.Should().Be(correctIndex);
        question.Points.Should().Be(points);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_GivenEmptyQuestionText_ShouldThrowArgumentException(string invalidText)
    {
        // Arrange
        var options = new List<string> { "Option 1", "Option 2" };

        // Act
        Action act = () => new Question("1", invalidText, options, 0, 1.0);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Constructor_GivenFewerThanTwoOptions_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new List<string> { "Only Option 1" };

        // Act
        Action act = () => new Question("1", "Valid question?", options, 0, 1.0);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*must have at least 2 options*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(5)]
    public void Constructor_GivenOutOfBoundsCorrectOptionIndex_ShouldThrowArgumentOutOfRangeException(int invalidIndex)
    {
        // Arrange
        var options = new List<string> { "Option 1", "Option 2" };

        // Act
        Action act = () => new Question("1", "Valid question?", options, invalidIndex, 1.0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage("*Correct option index is out of bounds*");
    }

    [Fact]
    public void Constructor_GivenNegativePoints_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var options = new List<string> { "Option 1", "Option 2" };

        // Act
        Action act = () => new Question("1", "Valid question?", options, 0, -0.5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage("*Points cannot be negative*");
    }
}
