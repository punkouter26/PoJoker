# Data Model: Digital Jester Core

**Feature**: 001-digital-jester-core  
**Date**: 2025-12-12  
**Status**: Complete

## Entity Relationship Overview

```
┌─────────────────┐       ┌──────────────────────┐
│      Joke       │───────│   JokePerformance    │
├─────────────────┤  1:N  ├──────────────────────┤
│ Id (string)     │       │ Id (Guid)            │
│ Setup (string)  │       │ JokeId (string)      │
│ Delivery        │       │ GuessedPunchline     │
│ Category        │       │ IsAiTriumph (bool)   │
│ Source          │       │ PerformedAt (DateTime)│
│ IsSafe (bool)   │       │ SessionId (Guid)     │
└─────────────────┘       │ Rating ──────────────┼──┐
                          └──────────────────────┘  │
                                                    │ 1:1 (embedded)
                          ┌──────────────────────┐  │
                          │     JokeRating       │◄─┘
                          ├──────────────────────┤
                          │ Cleverness (int 1-10)│
                          │ Rudeness (int 1-10)  │
                          │ Complexity (int 1-10)│
                          │ Difficulty (int 1-10)│
                          │ Commentary (string)  │
                          └──────────────────────┘

┌─────────────────────┐
│ PerformanceSession  │
├─────────────────────┤
│ Id (Guid)           │
│ StartedAt (DateTime)│
│ EndedAt (DateTime?) │
│ TotalJokes (int)    │
│ SafeModeEnabled     │
└─────────────────────┘
```

---

## Entities

### Joke

Represents a two-part joke fetched from the external JokeAPI.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | string | Required, Unique | External API joke ID (e.g., "232") |
| Setup | string | Required, Max 500 chars | The premise/question of the joke |
| Delivery | string | Required, Max 500 chars | The actual punchline |
| Category | string | Required, Enum-like | Programming, Misc, Dark, Pun, Spooky, Christmas |
| Source | string | Required | API source identifier (e.g., "jokeapi-v2") |
| IsSafe | bool | Required | True if joke passes safe-mode filter |

**Validation Rules**:
- Setup and Delivery must not be empty
- Category must be a known value
- Id must be unique within Source

---

### JokePerformance

Represents one complete joke cycle (Setup → Guess → Reveal → Analysis).

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | Required, PK | Unique performance identifier |
| JokeId | string | Required, FK | Reference to the Joke entity |
| GuessedPunchline | string | Max 500 chars, Nullable | AI's predicted punchline (null if stumped) |
| IsAiTriumph | bool | Required | True if guess matches delivery (case-insensitive, fuzzy match) |
| PerformedAt | DateTimeOffset | Required | UTC timestamp of performance |
| SessionId | Guid | Required, FK | Reference to PerformanceSession |
| Rating | JokeRating | Required, Embedded | Embedded rating object |

**State Transitions**:
```
[Created] → [GuessGenerated] → [Revealed] → [Analyzed] → [Persisted]
     │              │                            │
     └──[Stumped]───┘                            │
            (GuessedPunchline = null)            │
                                                 ▼
                                           [Displayed]
```

**Validation Rules**:
- PerformedAt cannot be in the future
- Rating must be present after analysis phase

---

### JokeRating

Embedded value object containing AI-generated scores and commentary.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Cleverness | int | 1-10 | Pun quality, logic twist rating |
| Rudeness | int | 1-10 | Insults, taboo subjects rating |
| Complexity | int | 1-10 | Domain knowledge required |
| Difficulty | int | 1-10 | How hard to predict the punchline |
| Commentary | string | Max 280 chars | Jester-persona witty remark |

**Validation Rules**:
- All scores must be integers between 1 and 10 inclusive
- Commentary must not be empty

---

### PerformanceSession

Container for a sequence of performances within a single user session.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | Required, PK | Unique session identifier |
| StartedAt | DateTimeOffset | Required | When the "Start" button was pressed |
| EndedAt | DateTimeOffset | Nullable | When the "Stop" button was pressed (null if active) |
| TotalJokes | int | >= 0 | Count of performances in this session |
| SafeModeEnabled | bool | Required | Whether safe mode was active for this session |

**Validation Rules**:
- EndedAt must be after StartedAt if set
- TotalJokes must match actual performance count

---

## Azure Table Storage Schema

### JokePerformanceEntity (Table: `jokeperformances`)

| Property | Type | Description |
|----------|------|-------------|
| PartitionKey | string | Category (e.g., "Programming") OR "Triumph" OR "Session_{id}" |
| RowKey | string | `{InvertedTimestamp}_{PerformanceId}` for descending sort |
| JokeId | string | External joke ID |
| Setup | string | Joke setup text |
| Delivery | string | Actual punchline |
| GuessedPunchline | string | AI's guess (empty if stumped) |
| IsAiTriumph | bool | Perfect match flag |
| Cleverness | int | Rating score |
| Rudeness | int | Rating score |
| Complexity | int | Rating score |
| Difficulty | int | Rating score |
| Commentary | string | Jester's remark |
| PerformedAt | DateTimeOffset | Timestamp |
| SessionId | Guid | Session reference |

**Inverted Timestamp Pattern**:
```csharp
string invertedTimestamp = (DateTimeOffset.MaxValue.Ticks - performedAt.Ticks).ToString("D19");
string rowKey = $"{invertedTimestamp}_{performanceId}";
```
This ensures newest performances appear first when querying in RowKey order.

---

## DTOs (Po.Joker.Shared)

### JokeDto
```csharp
public record JokeDto(
    string Id,
    string Setup,
    string Delivery,
    string Category,
    bool IsSafe
);
```

### JokePerformanceDto
```csharp
public record JokePerformanceDto(
    Guid Id,
    JokeDto Joke,
    string? GuessedPunchline,
    bool IsAiTriumph,
    JokeRatingDto Rating,
    DateTimeOffset PerformedAt
);
```

### JokeRatingDto
```csharp
public record JokeRatingDto(
    int Cleverness,
    int Rudeness,
    int Complexity,
    int Difficulty,
    string Commentary
);
```

### LeaderboardEntryDto
```csharp
public record LeaderboardEntryDto(
    string JokeSetup,
    string JokeDelivery,
    string? AiGuess,
    bool IsTriumph,
    int Score,          // The rating being sorted by
    string ScoreType,   // "Cleverness", "Rudeness", etc.
    DateTimeOffset PerformedAt
);
```

---

## Validation (FluentValidation)

### JokeValidator
```csharp
public class JokeValidator : AbstractValidator<JokeDto>
{
    private static readonly string[] ValidCategories = 
        ["Programming", "Misc", "Dark", "Pun", "Spooky", "Christmas"];
    
    public JokeValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Setup).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Delivery).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Category).Must(c => ValidCategories.Contains(c));
    }
}
```

### JokeRatingValidator
```csharp
public class JokeRatingValidator : AbstractValidator<JokeRatingDto>
{
    public JokeRatingValidator()
    {
        RuleFor(x => x.Cleverness).InclusiveBetween(1, 10);
        RuleFor(x => x.Rudeness).InclusiveBetween(1, 10);
        RuleFor(x => x.Complexity).InclusiveBetween(1, 10);
        RuleFor(x => x.Difficulty).InclusiveBetween(1, 10);
        RuleFor(x => x.Commentary).NotEmpty().MaximumLength(280);
    }
}
```
