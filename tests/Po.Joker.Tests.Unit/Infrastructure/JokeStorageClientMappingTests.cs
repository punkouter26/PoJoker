using FluentAssertions;
using Po.Joker.Infrastructure.Storage;
using Po.Joker.DTOs;
using Po.Joker.Enums;

namespace Po.Joker.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for JokeStorageClient mapping methods.
/// These tests ensure data integrity during DTO-Entity conversions.
/// </summary>
public class JokeStorageClientMappingTests
{
    [Fact]
    public void MapToEntity_WithCompletePerformanceDto_MapsAllProperties()
    {
        // Arrange
        var dto = CreateSamplePerformanceDto();

        // Act
        var entity = TestableJokeStorageClient.TestMapToEntity(dto);

        // Assert
        entity.Should().NotBeNull();
        entity.PartitionKey.Should().Be(dto.SessionId);
        entity.PerformanceId.Should().Be(dto.Id.ToString());
        entity.SessionId.Should().Be(dto.SessionId);
        entity.SequenceNumber.Should().Be(dto.SequenceNumber);
        
        // Joke properties
        entity.JokeId.Should().Be(dto.Joke.Id);
        entity.JokeCategory.Should().Be(dto.Joke.Category);
        entity.JokeSetup.Should().Be(dto.Joke.Setup);
        entity.JokePunchline.Should().Be(dto.Joke.Punchline);
        entity.SafeMode.Should().Be(dto.Joke.SafeMode);
        
        // Analysis properties
        entity.AiPunchline.Should().Be(dto.Analysis.AiPunchline);
        entity.Confidence.Should().Be(dto.Analysis.Confidence);
        entity.IsTriumph.Should().Be(dto.Analysis.IsTriumph);
        entity.SimilarityScore.Should().Be(dto.Analysis.SimilarityScore);
        entity.AiLatencyMs.Should().Be(dto.Analysis.LatencyMs);
        
        // Timestamps
        entity.StartedAt.Should().Be(dto.StartedAt);
        entity.CompletedAt.Should().Be(dto.CompletedAt);
        entity.DurationMs.Should().Be(dto.DurationMs);
    }

    [Fact]
    public void MapToEntity_WithRating_MapsRatingProperties()
    {
        // Arrange
        var dto = CreateSamplePerformanceDto();
        dto = dto with
        {
            Analysis = dto.Analysis with
            {
                Rating = new JokeRatingDto
                {
                    Cleverness = 8,
                    Rudeness = 3,
                    Complexity = 7,
                    Difficulty = 9,
                    Commentary = "Brilliant wordplay!"
                }
            }
        };

        // Act
        var entity = TestableJokeStorageClient.TestMapToEntity(dto);

        // Assert
        entity.RatingCleverness.Should().Be(8);
        entity.RatingRudeness.Should().Be(3);
        entity.RatingComplexity.Should().Be(7);
        entity.RatingDifficulty.Should().Be(9);
        entity.RatingCommentary.Should().Be("Brilliant wordplay!");
    }

    [Fact]
    public void MapToEntity_WithoutRating_SetsDefaultRatingValues()
    {
        // Arrange
        var dto = CreateSamplePerformanceDto();
        dto = dto with { Analysis = dto.Analysis with { Rating = null } };

        // Act
        var entity = TestableJokeStorageClient.TestMapToEntity(dto);

        // Assert
        entity.RatingCleverness.Should().Be(0);
        entity.RatingRudeness.Should().Be(0);
        entity.RatingComplexity.Should().Be(0);
        entity.RatingDifficulty.Should().Be(0);
        entity.RatingCommentary.Should().BeEmpty();
    }

    [Fact]
    public void MapToEntity_WithJokeFlags_MapsAllFlags()
    {
        // Arrange
        var dto = CreateSamplePerformanceDto();
        dto = dto with
        {
            Joke = dto.Joke with
            {
                Flags = new JokeFlags
                {
                    Nsfw = true,
                    Religious = false,
                    Political = true,
                    Racist = false,
                    Sexist = true,
                    Explicit = false
                }
            }
        };

        // Act
        var entity = TestableJokeStorageClient.TestMapToEntity(dto);

        // Assert
        entity.FlagNsfw.Should().BeTrue();
        entity.FlagReligious.Should().BeFalse();
        entity.FlagPolitical.Should().BeTrue();
        entity.FlagRacist.Should().BeFalse();
        entity.FlagSexist.Should().BeTrue();
        entity.FlagExplicit.Should().BeFalse();
    }

    [Fact]
    public void MapToDto_WithCompleteEntity_MapsAllProperties()
    {
        // Arrange
        var entity = CreateSampleEntity();

        // Act
        var dto = TestableJokeStorageClient.TestMapToDto(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.SessionId.Should().Be(entity.SessionId);
        dto.SequenceNumber.Should().Be(entity.SequenceNumber);
        
        // Joke properties
        dto.Joke.Id.Should().Be(entity.JokeId);
        dto.Joke.Category.Should().Be(entity.JokeCategory);
        dto.Joke.Setup.Should().Be(entity.JokeSetup);
        dto.Joke.Punchline.Should().Be(entity.JokePunchline);
        dto.Joke.SafeMode.Should().Be(entity.SafeMode);
        
        // Analysis properties
        dto.Analysis.AiPunchline.Should().Be(entity.AiPunchline);
        dto.Analysis.Confidence.Should().Be(entity.Confidence);
        dto.Analysis.IsTriumph.Should().Be(entity.IsTriumph);
        dto.Analysis.SimilarityScore.Should().Be(entity.SimilarityScore);
        dto.Analysis.LatencyMs.Should().Be(entity.AiLatencyMs);
        
        // Timestamps
        dto.StartedAt.Should().Be(entity.StartedAt);
        dto.CompletedAt.Should().Be(entity.CompletedAt);
    }

    [Fact]
    public void MapToDto_WithRatingData_RestoresRating()
    {
        // Arrange
        var entity = CreateSampleEntity();
        entity.RatingCleverness = 8;
        entity.RatingRudeness = 3;
        entity.RatingComplexity = 7;
        entity.RatingDifficulty = 9;
        entity.RatingCommentary = "Brilliant!";

        // Act
        var dto = TestableJokeStorageClient.TestMapToDto(entity);

        // Assert
        dto.Analysis.Rating.Should().NotBeNull();
        dto.Analysis.Rating!.Cleverness.Should().Be(8);
        dto.Analysis.Rating.Rudeness.Should().Be(3);
        dto.Analysis.Rating.Complexity.Should().Be(7);
        dto.Analysis.Rating.Difficulty.Should().Be(9);
        dto.Analysis.Rating.Commentary.Should().Be("Brilliant!");
    }

    [Fact]
    public void MapToDto_WithoutRatingData_HasNullRating()
    {
        // Arrange
        var entity = CreateSampleEntity();
        entity.RatingCleverness = 0;
        entity.RatingRudeness = 0;
        entity.RatingComplexity = 0;
        entity.RatingDifficulty = 0;

        // Act
        var dto = TestableJokeStorageClient.TestMapToDto(entity);

        // Assert
        dto.Analysis.Rating.Should().BeNull("all rating values are zero");
    }

    [Fact]
    public void MapToDto_WithJokeFlags_RestoresAllFlags()
    {
        // Arrange
        var entity = CreateSampleEntity();
        entity.FlagNsfw = true;
        entity.FlagReligious = false;
        entity.FlagPolitical = true;
        entity.FlagRacist = false;
        entity.FlagSexist = true;
        entity.FlagExplicit = false;

        // Act
        var dto = TestableJokeStorageClient.TestMapToDto(entity);

        // Assert
        dto.Joke.Flags.Nsfw.Should().BeTrue();
        dto.Joke.Flags.Religious.Should().BeFalse();
        dto.Joke.Flags.Political.Should().BeTrue();
        dto.Joke.Flags.Racist.Should().BeFalse();
        dto.Joke.Flags.Sexist.Should().BeTrue();
        dto.Joke.Flags.Explicit.Should().BeFalse();
    }

    [Fact]
    public void RoundTrip_MapsToEntityAndBack_PreservesData()
    {
        // Arrange
        var originalDto = CreateSamplePerformanceDto();

        // Act
        var entity = TestableJokeStorageClient.TestMapToEntity(originalDto);
        var resultDto = TestableJokeStorageClient.TestMapToDto(entity);

        // Assert
        resultDto.SessionId.Should().Be(originalDto.SessionId);
        resultDto.Joke.Id.Should().Be(originalDto.Joke.Id);
        resultDto.Joke.Setup.Should().Be(originalDto.Joke.Setup);
        resultDto.Analysis.IsTriumph.Should().Be(originalDto.Analysis.IsTriumph);
        resultDto.Analysis.Confidence.Should().Be(originalDto.Analysis.Confidence);
    }

    private static JokePerformanceDto CreateSamplePerformanceDto()
    {
        return new JokePerformanceDto
        {
            Id = Guid.NewGuid(),
            SessionId = "test-session",
            SequenceNumber = 1,
            Joke = new JokeDto
            {
                Id = 123,
                Category = "Programming",
                Type = "twopart",
                Setup = "Why do programmers prefer dark mode?",
                Punchline = "Because light attracts bugs!",
                SafeMode = true,
                Flags = new JokeFlags()
            },
            Analysis = new JokeAnalysisDto
            {
                Id = Guid.NewGuid(),
                OriginalJoke = new JokeDto { Id = 123, Category = "Programming", Type = "twopart" },
                AiPunchline = "Because they work at night",
                Confidence = 0.75,
                IsTriumph = false,
                SimilarityScore = 0.45,
                LatencyMs = 1500,
                AnalyzedAt = DateTimeOffset.UtcNow
            },
            StartedAt = DateTimeOffset.UtcNow.AddSeconds(-5),
            CompletedAt = DateTimeOffset.UtcNow,
            State = PerformanceState.Transitioning
        };
    }

    private static JokePerformanceEntity CreateSampleEntity()
    {
        return new JokePerformanceEntity
        {
            PartitionKey = "test-session",
            RowKey = "20231214120000_" + Guid.NewGuid().ToString("N"),
            PerformanceId = Guid.NewGuid().ToString(),
            SessionId = "test-session",
            SequenceNumber = 1,
            JokeId = 123,
            JokeCategory = "Programming",
            JokeType = "twopart",
            JokeSetup = "Why do programmers prefer dark mode?",
            JokePunchline = "Because light attracts bugs!",
            SafeMode = true,
            AiPunchline = "Because they work at night",
            Confidence = 0.75,
            IsTriumph = false,
            SimilarityScore = 0.45,
            AiLatencyMs = 1500,
            StartedAt = DateTimeOffset.UtcNow.AddSeconds(-5),
            CompletedAt = DateTimeOffset.UtcNow,
            DurationMs = 5000
        };
    }
}

/// <summary>
/// Testable wrapper to expose private mapping methods.
/// </summary>
internal static class TestableJokeStorageClient
{
    public static JokePerformanceEntity TestMapToEntity(JokePerformanceDto dto)
    {
        var rowKey = StorageConfiguration.GenerateRowKey(dto.CompletedAt, dto.Id);

        return new JokePerformanceEntity
        {
            PartitionKey = dto.SessionId,
            RowKey = rowKey,
            PerformanceId = dto.Id.ToString(),
            SessionId = dto.SessionId,
            SequenceNumber = dto.SequenceNumber,
            JokeId = dto.Joke.Id,
            JokeCategory = dto.Joke.Category,
            JokeType = dto.Joke.Type,
            JokeSetup = dto.Joke.Setup,
            JokePunchline = dto.Joke.Punchline,
            JokeText = dto.Joke.Joke,
            SafeMode = dto.Joke.SafeMode,
            AiPunchline = dto.Analysis.AiPunchline,
            Confidence = dto.Analysis.Confidence,
            IsTriumph = dto.Analysis.IsTriumph,
            SimilarityScore = dto.Analysis.SimilarityScore,
            AiLatencyMs = dto.Analysis.LatencyMs,
            StartedAt = dto.StartedAt,
            CompletedAt = dto.CompletedAt,
            DurationMs = dto.DurationMs,
            FlagNsfw = dto.Joke.Flags.Nsfw,
            FlagReligious = dto.Joke.Flags.Religious,
            FlagPolitical = dto.Joke.Flags.Political,
            FlagRacist = dto.Joke.Flags.Racist,
            FlagSexist = dto.Joke.Flags.Sexist,
            FlagExplicit = dto.Joke.Flags.Explicit,
            RatingCleverness = dto.Analysis.Rating?.Cleverness ?? 0,
            RatingRudeness = dto.Analysis.Rating?.Rudeness ?? 0,
            RatingComplexity = dto.Analysis.Rating?.Complexity ?? 0,
            RatingDifficulty = dto.Analysis.Rating?.Difficulty ?? 0,
            RatingAverage = dto.Analysis.Rating?.Average ?? 0.0,
            RatingCommentary = dto.Analysis.Rating?.Commentary ?? string.Empty
        };
    }

    public static JokePerformanceDto TestMapToDto(JokePerformanceEntity entity)
    {
        var joke = new JokeDto
        {
            Id = entity.JokeId,
            Category = entity.JokeCategory,
            Type = entity.JokeType,
            Setup = entity.JokeSetup,
            Punchline = entity.JokePunchline,
            Joke = entity.JokeText,
            SafeMode = entity.SafeMode,
            Flags = new JokeFlags
            {
                Nsfw = entity.FlagNsfw,
                Religious = entity.FlagReligious,
                Political = entity.FlagPolitical,
                Racist = entity.FlagRacist,
                Sexist = entity.FlagSexist,
                Explicit = entity.FlagExplicit
            }
        };

        JokeRatingDto? rating = null;
        if (entity.RatingCleverness > 0 || entity.RatingRudeness > 0 || entity.RatingComplexity > 0 || entity.RatingDifficulty > 0)
        {
            rating = new JokeRatingDto
            {
                Cleverness = entity.RatingCleverness,
                Rudeness = entity.RatingRudeness,
                Complexity = entity.RatingComplexity,
                Difficulty = entity.RatingDifficulty,
                Commentary = entity.RatingCommentary
            };
        }

        var analysis = new JokeAnalysisDto
        {
            Id = Guid.TryParse(entity.PerformanceId, out var id) ? id : Guid.NewGuid(),
            OriginalJoke = joke,
            AiPunchline = entity.AiPunchline,
            Confidence = entity.Confidence,
            IsTriumph = entity.IsTriumph,
            SimilarityScore = entity.SimilarityScore,
            LatencyMs = entity.AiLatencyMs,
            AnalyzedAt = entity.CompletedAt,
            Rating = rating
        };

        return new JokePerformanceDto
        {
            Id = Guid.TryParse(entity.PerformanceId, out var perfId) ? perfId : Guid.NewGuid(),
            SessionId = entity.SessionId,
            Joke = joke,
            Analysis = analysis,
            SequenceNumber = entity.SequenceNumber,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            State = PerformanceState.Transitioning
        };
    }
}
