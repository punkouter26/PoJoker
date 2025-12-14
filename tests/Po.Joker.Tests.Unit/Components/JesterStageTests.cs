using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Po.Joker.Client.Components;
using Po.Joker.Client.Services;
using Po.Joker.Shared.DTOs;
using Po.Joker.Shared.Enums;

namespace Po.Joker.Tests.Unit.Components;

/// <summary>
/// TDD tests for JesterStage.razor component - Written FIRST before implementation.
/// Tests the 5-act performance state machine.
/// </summary>
public class JesterStageTests : BunitContext
{
    private readonly Mock<ISpeechService> _mockSpeechService;
    private readonly Mock<IAudioService> _mockAudioService;

    public JesterStageTests()
    {
        _mockSpeechService = new Mock<ISpeechService>();
        _mockAudioService = new Mock<IAudioService>();

        // Register mock services
        Services.AddSingleton(_mockSpeechService.Object);
        Services.AddSingleton(_mockAudioService.Object);
    }

    [Fact]
    public void JesterStage_InitialState_ShowsIdleMessage()
    {
        // Arrange & Act
        var cut = Render<JesterStage>();

        // Assert
        cut.Markup.Should().Contain("Start");
        cut.Find(".jester-stage").Should().NotBeNull();
    }

    [Fact]
    public void JesterStage_WhenStartClicked_BeginsPerformance()
    {
        // Arrange
        var cut = Render<JesterStage>();

        // Act
        cut.Find("button.btn-primary").Click();

        // Assert - Should transition to fetching state
        cut.WaitForState(() => cut.Markup.Contains("Fetching") || cut.Markup.Contains("loading"));
    }

}
