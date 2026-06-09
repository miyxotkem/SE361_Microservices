using System;

namespace BuildingBlocks.Messaging.Events;

public record ExamPublishedEvent : IntegrationEvent
{
    public string ExamId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
}
