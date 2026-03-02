using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Po.Joker.Application;
using Po.Joker.Components;
using Po.Joker.Contracts;
using Po.Joker.Services;
using Po.Joker.DTOs;
using Po.Joker.Enums;

namespace Po.Joker.Tests.Unit.Components;

/// <summary>
/// TDD tests for JesterStage.razor component - Written FIRST before implementation.
/// Tests the 5-act performance state machine.
/// </summary>
public class JesterStageTests : BunitContext
{
    public JesterStageTests()
    {
        // Register interface-mapped services required by JesterStage
        Services.AddScoped<IAudioService, NullAudioService>();
        Services.AddScoped<ISpeechService, NullSpeechService>();
        Services.AddOptions<PerformanceSettings>();
    }

    [Fact]
    public void JesterStage_InitialState_ShowsIdleMessage()
    {
        // Arrange & Act
        var cut = Render<JesterStage>();

        // Assert
        cut.Markup.Should().Contain("Begin");
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
