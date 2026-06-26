using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;
using FluentAssertions;
using Xunit;
using Course.API.Features.Courses;

namespace Course.API.Tests;

public class CourseHelperTests
{
    [Fact]
    public void ConvertFirestoreTypes_GivenPlainValue_ShouldReturnSameValue()
    {
        // Arrange
        var val = "hello";

        // Act
        var result = CourseHelper.ConvertFirestoreTypes(val);

        // Assert
        result.Should().Be(val);
    }

    [Fact]
    public void ConvertFirestoreTypes_GivenTimestamp_ShouldReturnUniversalDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2026, 6, 26, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = Timestamp.FromDateTime(dateTime);

        // Act
        var result = CourseHelper.ConvertFirestoreTypes(timestamp);

        // Assert
        result.Should().BeOfType<DateTime>();
        ((DateTime)result).Should().Be(dateTime);
    }

    [Fact]
    public void ConvertFirestoreTypes_GivenNestedDictionaryAndList_ShouldConvertRecursively()
    {
        // Arrange
        var dateTime = new DateTime(2026, 6, 26, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = Timestamp.FromDateTime(dateTime);

        var dict = new Dictionary<string, object>
        {
            { "name", "Course 1" },
            { "created", timestamp },
            { "tags", new List<object> { "education", timestamp } }
        };

        // Act
        var result = CourseHelper.ConvertFirestoreTypes(dict);

        // Assert
        result.Should().BeOfType<Dictionary<string, object>>();
        var convertedDict = (Dictionary<string, object>)result;
        convertedDict["name"].Should().Be("Course 1");
        convertedDict["created"].Should().Be(dateTime);

        var convertedTags = (List<object>)convertedDict["tags"];
        convertedTags[0].Should().Be("education");
        convertedTags[1].Should().Be(dateTime);
    }
}
