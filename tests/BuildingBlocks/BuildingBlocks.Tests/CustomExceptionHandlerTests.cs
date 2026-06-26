using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Exceptions.Handler;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BuildingBlocks.Tests;

public class CustomExceptionHandlerTests
{
    private readonly ILogger<CustomExceptionHandler> _loggerMock;
    private readonly CustomExceptionHandler _handler;

    public CustomExceptionHandlerTests()
    {
        _loggerMock = Substitute.For<ILogger<CustomExceptionHandler>>();
        _handler = new CustomExceptionHandler(_loggerMock);
    }

    [Theory]
    [InlineData(typeof(BadRequestException), 400)]
    [InlineData(typeof(NotFoundException), 404)]
    [InlineData(typeof(InternalServerException), 500)]
    [InlineData(typeof(Exception), 500)]
    public async Task TryHandleAsync_ShouldMapExceptionsToCorrectStatusCodes(Type exceptionType, int expectedStatusCode)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        Exception exceptionInstance;
        if (exceptionType == typeof(BadRequestException))
            exceptionInstance = new BadRequestException("Bad request test message");
        else if (exceptionType == typeof(NotFoundException))
            exceptionInstance = new NotFoundException("Not found test message");
        else if (exceptionType == typeof(InternalServerException))
            exceptionInstance = new InternalServerException("Internal server test message");
        else
            exceptionInstance = new Exception("General test message");

        // Act
        var result = await _handler.TryHandleAsync(context, exceptionInstance, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(expectedStatusCode);

        // Read response body
        responseStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseStream);
        var responseContent = await reader.ReadToEndAsync();
        
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseContent, jsonOptions);

        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be(exceptionInstance.GetType().Name);
        problemDetails.Detail.Should().Be(exceptionInstance.Message);
        problemDetails.Status.Should().Be(expectedStatusCode);
    }
}
