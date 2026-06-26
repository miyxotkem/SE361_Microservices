using System;
using System.Threading.Tasks;
using BuildingBlocks.Messaging.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Notification.API.EventBusConsumer;

namespace Notification.API.Tests;

public class EnrollmentActivatedConsumerTests
{
    private readonly ILogger<EnrollmentActivatedConsumer> _loggerMock;
    private readonly EnrollmentActivatedConsumer _consumer;

    public EnrollmentActivatedConsumerTests()
    {
        _loggerMock = Substitute.For<ILogger<EnrollmentActivatedConsumer>>();
        // Create consumer with null FirestoreDb for basic unit tests
        _consumer = new EnrollmentActivatedConsumer(null!, _loggerMock);
    }

    [Fact]
    public async Task Consume_ShouldThrowNullReferenceException_WhenFirestoreDbIsNull()
    {
        // Arrange
        var contextMock = Substitute.For<ConsumeContext<EnrollmentActivatedEvent>>();
        var message = new EnrollmentActivatedEvent
        {
            Id = Guid.NewGuid(),
            UserId = "student-1",
            CourseId = "course-1",
            OccurredOn = DateTime.UtcNow
        };
        contextMock.Message.Returns(message);

        // Act
        Func<Task> act = async () => await _consumer.Consume(contextMock);

        // Assert
        // Should log information before attempting to save to Firestore
        await act.Should().ThrowAsync<NullReferenceException>();
        
        _loggerMock.ReceivedWithAnyArgs().LogInformation(default);
    }
}
