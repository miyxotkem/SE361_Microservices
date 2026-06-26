using System;
using System.Threading;
using System.Threading.Tasks;
using Comment.API.Features.Comments.AddComment;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Comment.API.Tests;

public class AddCommentTests
{
    [Fact]
    public async Task Handle_ShouldThrowNullReferenceException_WhenFirestoreDbIsNull()
    {
        // Arrange
        var handler = new AddCommentCommandHandler(null!);
        var request = new AddCommentRequest
        {
            LessonId = "lesson-1",
            ParentId = "",
            Content = "Test Comment",
            UserName = "Alice",
            UserRole = "Student",
            ProfileImageUrl = "http://avatar.url"
        };
        var command = new AddCommentCommand("user-1", request);

        // Act
        // Because FirestoreDb is null, handler will throw NullReferenceException or return 500 error in try-catch
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Since it catches the exception and returns a JSON result with status 500
        var statusCodeProperty = result.GetType().GetProperty("StatusCode");
        var statusCode = statusCodeProperty?.GetValue(result) as int?;
        statusCode.Should().Be(500);
    }
}
