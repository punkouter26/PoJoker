using FluentAssertions;
using Po.Joker.Features.Diagnostics;

namespace Po.Joker.Tests.Unit.Features;

/// <summary>
/// Unit tests for ConfigMasker.Mask() which hides middle portion of secret values.
/// </summary>
public sealed class ConfigMaskerTests
{
    [Fact]
    public void Mask_NullValue_ReturnsEmpty()
    {
        ConfigMasker.Mask(null).Should().Be("(empty)");
    }

    [Fact]
    public void Mask_EmptyString_ReturnsEmpty()
    {
        ConfigMasker.Mask("").Should().Be("(empty)");
    }

    [Theory]
    [InlineData("a", "*")]
    [InlineData("ab", "**")]
    [InlineData("abc", "***")]
    [InlineData("abcd", "****")]
    public void Mask_ShortValues_FullyMasked(string input, string expected)
    {
        ConfigMasker.Mask(input).Should().Be(expected);
    }

    [Fact]
    public void Mask_FiveCharValue_ShowsFirstTwoAndLastTwo()
    {
        // "hello" → "he*lo"
        var result = ConfigMasker.Mask("hello");
        result.Should().StartWith("he");
        result.Should().EndWith("lo");
        result.Should().Contain("*");
        result.Length.Should().Be(5); // 2 + 1 + 2
    }

    [Fact]
    public void Mask_LongValue_CapsMiddleAt20Stars()
    {
        var longValue = new string('x', 100);
        var result = ConfigMasker.Mask(longValue);
        result.Should().StartWith("xx");
        result.Should().EndWith("xx");
        // Middle is capped at 20 stars
        result.Length.Should().Be(24); // 2 + 20 + 2
    }

    [Fact]
    public void Mask_ConnectionString_MasksMiddle()
    {
        var connStr = "Server=myserver.database.windows.net;Database=mydb;User Id=admin;Password=secret123";
        var result = ConfigMasker.Mask(connStr);
        result.Should().StartWith("Se");
        result.Should().EndWith("23");
        result.Should().Contain("*");
    }

    [Fact]
    public void Mask_ApiKey_MasksMiddle()
    {
        var key = "sk-proj-abc123def456";
        var result = ConfigMasker.Mask(key);
        result.Should().StartWith("sk");
        result.Should().EndWith("56");
    }
}
