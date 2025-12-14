# Research: Digital Jester Core

**Feature**: 001-digital-jester-core  
**Date**: 2025-12-12  
**Status**: Complete

## External API Research

### JokeAPI (https://v2.jokeapi.dev/)

**Decision**: Use JokeAPI as primary joke source

**Rationale**:
- Free tier with 120 req/min rate limit (sufficient for passive loop)
- Two-part joke format (`setup` + `delivery`) matches spec requirements
- Built-in `safe-mode` parameter for FR-027 Safe Mode toggle
- Category filtering via `?category=Programming,Misc,Pun`
- No API key required for basic usage

**Alternatives Considered**:
- icanhazdadjoke.com: Single-line jokes only (no setup/delivery split)
- Official Joke API: Smaller dataset, less category variety
- Chuck Norris API: Single category, not suitable for variety

**Integration Pattern**:
```
GET https://v2.jokeapi.dev/joke/Any?type=twopart&safe-mode
```

**Error Handling**:
- 429 Rate Limited → Exponential backoff with Polly
- 5xx Server Error → Retry 3x, then circuit breaker
- Empty response → Re-fetch with different category

---

### Azure OpenAI (gpt-35-turbo)

**Decision**: Use gpt-35-turbo via Azure AI Foundry Hub/Project architecture

**Rationale**:
- Cost: ~$0.002/1K tokens vs $0.03/1K for gpt-4
- Sufficient reasoning capability for punchline prediction
- Azure-native integration with Managed Identity (secret-less)
- 15-second timeout aligns with performance budget

**Alternatives Considered**:
- gpt-4: 15x more expensive, overkill for humor prediction
- Claude: No native Azure integration, requires separate API key
- Local LLM: Compute cost exceeds $5/month budget

**Prompt Strategy**:
```
System: You are a witty court jester. Given a joke setup, predict the punchline.
User: Setup: "{setup}"
Respond with ONLY the predicted punchline, nothing else.
```

**Analysis Prompt**:
```
System: You are a comedy critic. Rate this joke on a scale of 1-10 for:
- Cleverness (pun quality, logic twist)
- Rudeness (insults, taboo subjects)  
- Complexity (domain knowledge required)
- Difficulty (how hard to guess the punchline)

Also provide a short witty comment in character as a medieval jester.

User: Setup: "{setup}"
Delivery: "{delivery}"
AI Guess: "{guess}"

Respond in JSON: {"cleverness":N,"rudeness":N,"complexity":N,"difficulty":N,"comment":"..."}
```

---

### Web Speech API

**Decision**: Use browser-native Web Speech API for TTS

**Rationale**:
- Zero cost (no API calls)
- Offline-capable
- Sufficient voice quality for entertainment
- British male voice available in Chrome/Edge

**Voice Selection Logic**:
```javascript
const voices = speechSynthesis.getVoices();
const britishMale = voices.find(v => 
  v.lang === 'en-GB' && v.name.toLowerCase().includes('male')
) || voices.find(v => v.lang === 'en-GB') 
  || voices.find(v => v.lang.startsWith('en'));
```

**Alternatives Considered**:
- Azure Speech Services: $1/audio hour, exceeds budget for continuous use
- ElevenLabs: High quality but $5/month minimum
- Amazon Polly: Requires AWS integration, adds complexity

---

### Web Audio API

**Decision**: Generate audio cues programmatically (no external assets)

**Rationale**:
- Zero hosting cost for audio files
- Precise timing control for drum roll
- Smaller bundle size
- Works offline

**Drum Roll Implementation**:
```javascript
function playDrumRoll(audioContext, durationMs = 2000) {
  const oscillator = audioContext.createOscillator();
  const gainNode = audioContext.createGain();
  oscillator.type = 'triangle';
  oscillator.frequency.setValueAtTime(100, audioContext.currentTime);
  oscillator.frequency.exponentialRampToValueAtTime(400, audioContext.currentTime + durationMs/1000);
  gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
  gainNode.gain.exponentialRampToValueAtTime(0.8, audioContext.currentTime + durationMs/1000);
  oscillator.connect(gainNode).connect(audioContext.destination);
  oscillator.start();
  oscillator.stop(audioContext.currentTime + durationMs/1000);
}
```

**Trombone Slide Implementation**:
```javascript
function playTromboneSlide(audioContext) {
  const oscillator = audioContext.createOscillator();
  oscillator.type = 'sawtooth';
  oscillator.frequency.setValueAtTime(300, audioContext.currentTime);
  oscillator.frequency.exponentialRampToValueAtTime(80, audioContext.currentTime + 1);
  oscillator.connect(audioContext.destination);
  oscillator.start();
  oscillator.stop(audioContext.currentTime + 1);
}
```

---

## Storage Strategy

### Azure Table Storage

**Decision**: Use Azure Table Storage instead of SQL/EF Core

**Rationale**:
- Cost: ~$0.04/GB/month vs $5+/month for Azure SQL
- Sufficient for leaderboard queries (partition by Category)
- No schema migrations needed
- Native Azure SDK with Managed Identity support

**Partition Strategy**:
| PartitionKey | RowKey | Use Case |
|--------------|--------|----------|
| `Category` (e.g., "Programming") | `{Timestamp}_{JokeId}` | Category leaderboards |
| `Session_{SessionId}` | `{Timestamp}` | Session history |
| `Triumph` | `{Timestamp}_{JokeId}` | AI Triumph leaderboard |

**Query Patterns**:
- "Most Clever in Programming": Filter PartitionKey="Programming", sort by Cleverness desc, take 10
- "AI Triumphs": Filter PartitionKey="Triumph", take 10
- "Session History": Filter PartitionKey="Session_{id}", order by RowKey

**Alternatives Considered**:
- Azure SQL: Exceeds $5/month budget
- Cosmos DB: Overkill for simple queries, RU cost unpredictable
- Browser localStorage only: No cross-device sync (acceptable per clarification)

---

## Best Practices Applied

### Polly Resilience Pipelines

Per Constitution III, all external HTTP calls use Polly:

```csharp
services.AddResiliencePipeline("joke-api", builder =>
{
    builder
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.ExponentialWithJitter,
            UseJitter = true
        })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromSeconds(30)
        })
        .AddTimeout(TimeSpan.FromSeconds(10));
});

services.AddResiliencePipeline("azure-openai", builder =>
{
    builder
        .AddTimeout(TimeSpan.FromSeconds(15)) // Hard timeout for AI
        .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 2 })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.3,
            BreakDuration = TimeSpan.FromMinutes(1)
        });
});
```

### Global Exception Handling

Per Constitution III, RFC 7807 with Jester theming:

```csharp
public class JesterExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var problem = new ProblemDetails
        {
            Status = 500,
            Title = "The Jester Has Tripped!",
            Detail = GetJesterMessage(exception),
            Instance = context.Request.Path
        };
        problem.Extensions["jesterCode"] = exception.GetType().Name;
        
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
    
    private string GetJesterMessage(Exception ex) => ex switch
    {
        HttpRequestException => "The Royal Scroll is missing!",
        TimeoutException => "The courier took too long!",
        _ => "The Jester has stumbled over his shoelaces!"
    };
}
```

---

## Unresolved Items

None. All technical decisions are finalized for Phase 1.
