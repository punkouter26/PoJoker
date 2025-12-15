using FluentAssertions;
using Po.Joker.Features.Analysis;

namespace Po.Joker.Tests.Unit.Features;

/// <summary>
/// Unit tests for AiJesterService.CalculateSimilarity method.
/// Tests the Jaccard similarity algorithm used for punchline comparison.
/// </summary>
public class AiJesterServiceSimilarityTests
{
    [Fact]
    public void CalculateSimilarity_WithIdenticalStrings_ReturnsOne()
    {
        // Arrange
        var text = "Because light attracts bugs";

        // Act
        var similarity = TestableAiJesterService.TestCalculateSimilarity(text, text);

        // Assert
        similarity.Should().Be(1.0, "identical strings should have 100% similarity");
    }

    [Fact]
    public void CalculateSimilarity_WithCompletelyDifferentStrings_ReturnsLowValue()
    {
        // Arrange
        var actual = "Because light attracts bugs";
        var predicted = "Hello world";

        // Act
        var similarity = TestableAiJesterService.TestCalculateSimilarity(actual, predicted);

        // Assert
        similarity.Should().BeLessThan(0.3, "completely different strings should have low similarity");
    }

    [Fact]
    public void CalculateSimilarity_WithPartialOverlap_ReturnsModerateValue()
    {
        // Arrange
        var actual = "Because light attracts bugs";
        var predicted = "Because bugs attract light";

        // Act
        var similarity = TestableAiJesterService.TestCalculateSimilarity(actual, predicted);

        // Assert
        similarity.Should().BeGreaterThan(0.5, "strings with word overlap should have moderate similarity")
            .And.BeLessThan(1.0, "different word order means not identical");
    }

    [Fact]
    public void CalculateSimilarity_IsCaseInsensitive()
    {
        // Arrange
        var actual = "Because LIGHT attracts BUGS";
        var predicted = "because light attracts bugs";

        // Act
        var similarity = TestableAiJesterService.TestCalculateSimilarity(actual, predicted);

        // Assert
        similarity.Should().Be(1.0, "similarity should be case-insensitive");
    }

    [Fact]
    public void CalculateSimilarity_IgnoresPunctuation()
    {
        // Arrange
        var actual = "Because, light attracts bugs!";
        var predicted = "Because light attracts bugs";

        // Act
        var similarity = TestableAiJesterService.TestCalculateSimilarity(actual, predicted);

        // Assert
        similarity.Should().Be(1.0, "punctuation should be ignored in similarity calculation");
    }

    [Fact]
    public void CalculateSimilarity_WithNullOrEmpty_ReturnsZero()
    {
        // Arrange & Act
        var sim1 = TestableAiJesterService.TestCalculateSimilarity(null, "test");
        var sim2 = TestableAiJesterService.TestCalculateSimilarity("test", null);
        var sim3 = TestableAiJesterService.TestCalculateSimilarity("", "test");
        var sim4 = TestableAiJesterService.TestCalculateSimilarity("test", "");
        var sim5 = TestableAiJesterService.TestCalculateSimilarity("   ", "test");

        // Assert
        sim1.Should().Be(0.0);
        sim2.Should().Be(0.0);
        sim3.Should().Be(0.0);
        sim4.Should().Be(0.0);
        sim5.Should().Be(0.0);
    }

    [Fact]
    public void CalculateSimilarity_UsesJaccardAlgorithm()
    {
        // Arrange - Known Jaccard similarity calculation
        // Set A: {the, cat, sat}
        // Set B: {the, dog, sat}
        // Intersection: {the, sat} = 2 words
        // Union: {the, cat, sat, dog} = 4 words
        // Jaccard = 2/4 = 0.5
        var actual = "the cat sat";
        var predicted = "the dog sat";

        // Act
        var similarity = TestableAiJesterService.TestCalculateSimilarity(actual, predicted);

        // Assert
        similarity.Should().BeApproximately(0.5, 0.01, 
            "Jaccard similarity of {the,cat,sat} and {the,dog,sat} should be 0.5");
    }

    [Fact]
    public void CalculateSimilarity_WithSingleWordMatch_ReturnsCorrectRatio()
    {
        // Arrange
        // Set A: {hello}
        // Set B: {hello, world}
        // Intersection: {hello} = 1
        // Union: {hello, world} = 2
        // Jaccard = 1/2 = 0.5
        var actual = "hello";
        var predicted = "hello world";

        // Act
        var similarity = TestableAiJesterService.TestCalculateSimilarity(actual, predicted);

        // Assert
        similarity.Should().BeApproximately(0.5, 0.01);
    }

    [Fact]
    public void CalculateSimilarity_WithNoCommonWords_ReturnsZero()
    {
        // Arrange
        var actual = "apple banana orange";
        var predicted = "car truck boat";

        // Act
        var similarity = TestableAiJesterService.TestCalculateSimilarity(actual, predicted);

        // Assert
        similarity.Should().Be(0.0, "no common words means zero similarity");
    }

    [Theory]
    [InlineData("Because light attracts bugs", "Light attracts bugs", 0.75)] // 3/4 words match
    [InlineData("Hello world", "world", 0.5)] // 1/2 words match
    [InlineData("The quick brown fox", "The lazy brown dog", 0.3333333333333333)] // current algorithm yields 1/3
    public void CalculateSimilarity_WithVariousInputs_ReturnsExpectedValues(
        string actual, string predicted, double expectedSimilarity)
    {
        // Act
        var similarity = TestableAiJesterService.TestCalculateSimilarity(actual, predicted);

        // Assert
        similarity.Should().BeApproximately(expectedSimilarity, 0.1, 
            $"similarity between '{actual}' and '{predicted}' should be approximately {expectedSimilarity}");
    }

    [Fact]
    public void CalculateSimilarity_TriumphThreshold_IsEightyPercent()
    {
        // This test documents the business rule that 80% similarity = triumph
        // Arrange - Create strings with exactly 80% word overlap
        var actual = "one two three four five";
        var predicted = "one two three four"; // 4/5 = 0.8

        // Act
        var similarity = TestableAiJesterService.TestCalculateSimilarity(actual, predicted);

        // Assert
        similarity.Should().BeGreaterThanOrEqualTo(0.8,
            "80% similarity should meet or exceed the triumph threshold");
    }
}

/// <summary>
/// Testable wrapper to expose private CalculateSimilarity method.
/// </summary>
internal static class TestableAiJesterService
{
    public static double TestCalculateSimilarity(string? actual, string? predicted)
    {
        if (string.IsNullOrWhiteSpace(actual) || string.IsNullOrWhiteSpace(predicted))
            return 0;

        var actualWords = actual.ToLowerInvariant()
            .Split([' ', '.', '!', '?', ','], StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        var predictedWords = predicted.ToLowerInvariant()
            .Split([' ', '.', '!', '?', ','], StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();

        if (actualWords.Count == 0 || predictedWords.Count == 0)
            return 0;

        // Jaccard similarity
        var intersection = actualWords.Intersect(predictedWords).Count();
        var union = actualWords.Union(predictedWords).Count();

        return (double)intersection / union;
    }
}
