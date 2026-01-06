using FluentAssertions;
using Freecost.Core.Enums;
using Freecost.Core.Services;
using Xunit;

namespace Freecost.Tests.Services;

public class StatusNotificationServiceTests
{
    private readonly StatusNotificationService _sut;

    public StatusNotificationServiceTests()
    {
        _sut = new StatusNotificationService();
    }

    [Fact]
    public void ShowSuccess_ShouldTriggerNotificationWithSuccessLevel()
    {
        // Arrange
        var message = "Operation completed successfully";
        NotificationLevel? capturedLevel = null;
        string? capturedMessage = null;
        int? capturedDuration = null;

        _sut.NotificationPosted += (sender, args) =>
        {
            capturedLevel = args.Level;
            capturedMessage = args.Message;
            capturedDuration = args.DurationMs;
        };

        // Act
        _sut.ShowSuccess(message);

        // Assert
        capturedLevel.Should().Be(NotificationLevel.Success);
        capturedMessage.Should().Be(message);
        capturedDuration.Should().Be(3000);
    }

    [Fact]
    public void ShowInfo_ShouldTriggerNotificationWithInfoLevel()
    {
        // Arrange
        var message = "This is information";
        NotificationLevel? capturedLevel = null;
        string? capturedMessage = null;
        int? capturedDuration = null;

        _sut.NotificationPosted += (sender, args) =>
        {
            capturedLevel = args.Level;
            capturedMessage = args.Message;
            capturedDuration = args.DurationMs;
        };

        // Act
        _sut.ShowInfo(message);

        // Assert
        capturedLevel.Should().Be(NotificationLevel.Info);
        capturedMessage.Should().Be(message);
        capturedDuration.Should().Be(3000);
    }

    [Fact]
    public void ShowWarning_ShouldTriggerNotificationWithWarningLevel()
    {
        // Arrange
        var message = "This is a warning";
        NotificationLevel? capturedLevel = null;
        string? capturedMessage = null;
        int? capturedDuration = null;

        _sut.NotificationPosted += (sender, args) =>
        {
            capturedLevel = args.Level;
            capturedMessage = args.Message;
            capturedDuration = args.DurationMs;
        };

        // Act
        _sut.ShowWarning(message);

        // Assert
        capturedLevel.Should().Be(NotificationLevel.Warning);
        capturedMessage.Should().Be(message);
        capturedDuration.Should().Be(5000);
    }

    [Fact]
    public void ShowError_ShouldTriggerNotificationWithErrorLevel()
    {
        // Arrange
        var message = "An error occurred";
        NotificationLevel? capturedLevel = null;
        string? capturedMessage = null;
        int? capturedDuration = null;

        _sut.NotificationPosted += (sender, args) =>
        {
            capturedLevel = args.Level;
            capturedMessage = args.Message;
            capturedDuration = args.DurationMs;
        };

        // Act
        _sut.ShowError(message);

        // Assert
        capturedLevel.Should().Be(NotificationLevel.Error);
        capturedMessage.Should().Be(message);
        capturedDuration.Should().Be(0); // Errors persist until dismissed
    }

    [Fact]
    public void ShowSuccess_WithCustomDuration_ShouldUseCustomDuration()
    {
        // Arrange
        var message = "Success with custom duration";
        var customDuration = 10000;
        int? capturedDuration = null;

        _sut.NotificationPosted += (sender, args) =>
        {
            capturedDuration = args.DurationMs;
        };

        // Act
        _sut.ShowSuccess(message, customDuration);

        // Assert
        capturedDuration.Should().Be(customDuration);
    }

    [Fact]
    public void ShowWarning_WithCustomDuration_ShouldUseCustomDuration()
    {
        // Arrange
        var message = "Warning with custom duration";
        var customDuration = 8000;
        int? capturedDuration = null;

        _sut.NotificationPosted += (sender, args) =>
        {
            capturedDuration = args.DurationMs;
        };

        // Act
        _sut.ShowWarning(message, customDuration);

        // Assert
        capturedDuration.Should().Be(customDuration);
    }

    [Fact]
    public void NotificationPosted_ShouldIncludeTimestamp()
    {
        // Arrange
        var message = "Test notification";
        DateTime? capturedTimestamp = null;
        var beforeNotification = DateTime.Now;

        _sut.NotificationPosted += (sender, args) =>
        {
            capturedTimestamp = args.Timestamp;
        };

        // Act
        _sut.ShowInfo(message);

        // Assert
        capturedTimestamp.Should().NotBeNull();
        capturedTimestamp.Should().BeOnOrAfter(beforeNotification);
        capturedTimestamp.Should().BeOnOrBefore(DateTime.Now);
    }

    [Fact]
    public void MultipleNotifications_ShouldTriggerEventMultipleTimes()
    {
        // Arrange
        var notificationCount = 0;
        var capturedMessages = new List<string>();

        _sut.NotificationPosted += (sender, args) =>
        {
            notificationCount++;
            capturedMessages.Add(args.Message);
        };

        // Act
        _sut.ShowSuccess("First");
        _sut.ShowInfo("Second");
        _sut.ShowWarning("Third");
        _sut.ShowError("Fourth");

        // Assert
        notificationCount.Should().Be(4);
        capturedMessages.Should().HaveCount(4);
        capturedMessages.Should().ContainInOrder("First", "Second", "Third", "Fourth");
    }

    [Fact]
    public void Notification_WithNoSubscribers_ShouldNotThrow()
    {
        // Arrange
        var message = "Test with no subscribers";

        // Act
        Action act = () => _sut.ShowSuccess(message);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Notification_WithMultipleSubscribers_ShouldNotifyAll()
    {
        // Arrange
        var message = "Notify all subscribers";
        var subscriber1Called = false;
        var subscriber2Called = false;

        _sut.NotificationPosted += (sender, args) =>
        {
            subscriber1Called = true;
        };

        _sut.NotificationPosted += (sender, args) =>
        {
            subscriber2Called = true;
        };

        // Act
        _sut.ShowSuccess(message);

        // Assert
        subscriber1Called.Should().BeTrue();
        subscriber2Called.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A very long message that might be used in the application to test how the notification system handles lengthy text content")]
    public void ShowNotification_WithVariousMessageLengths_ShouldTriggerEvent(string message)
    {
        // Arrange
        string? capturedMessage = null;

        _sut.NotificationPosted += (sender, args) =>
        {
            capturedMessage = args.Message;
        };

        // Act
        _sut.ShowInfo(message);

        // Assert
        capturedMessage.Should().Be(message);
    }
}
