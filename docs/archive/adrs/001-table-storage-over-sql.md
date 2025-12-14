# ADR-001: Table Storage Over SQL Server

**Date**: 2025-01-01  
**Status**: Accepted  
**Deciders**: Po.Joker Development Team

## Context

Po.Joker requires persistent storage for:
1. **Joke Performance Data**: AI predictions, ratings (Cleverness, Rudeness, Complexity, Difficulty), and commentary
2. **Leaderboard Entries**: Historical joke rankings sorted by various rating categories
3. **Session Tracking**: Browser session state and duplicate joke detection

We evaluated the following storage options:
- Azure SQL Database
- Azure Cosmos DB
- Azure Table Storage
- Azure Blob Storage with JSON files

## Decision

We have decided to use **Azure Table Storage** for all persistence needs.

## Rationale

### Cost Efficiency

| Service | Monthly Cost (Small Scale) | Notes |
|---------|---------------------------|-------|
| Azure SQL Database | ~$5-15 | Basic tier, 2GB |
| Cosmos DB | ~$25 | 400 RU/s minimum |
| **Table Storage** | **~$0.10-0.50** | Pay per GB + transactions |

Table Storage is 10-50x cheaper for our use case with modest data volumes (~100 jokes/day).

### Data Access Patterns

Our access patterns are well-suited to key-value storage:

1. **Write Pattern**: Single joke at a time after AI analysis
2. **Read Pattern**: 
   - Leaderboards: Query by partition (category), sorted by RowKey (inverted timestamp/score)
   - Session: Point reads by session ID
   - Diagnostics: Aggregate counts

### Schema Design

```
Table: JokePerformances
├── PartitionKey: "{Category}_{Year-Month}"  // e.g., "cleverness_2025-01"
├── RowKey: "{InvertedTimestamp}_{JokeId}"   // Latest first ordering
├── Setup: string
├── Delivery: string  
├── AiPrediction: string
├── WasCorrect: bool
├── Cleverness: int (1-10)
├── Rudeness: int (1-10)
├── Complexity: int (1-10)
├── Difficulty: int (1-10)
├── Commentary: string
└── Timestamp: DateTimeOffset
```

**Partition Strategy Benefits**:
- Leaderboards query single partition (`cleverness_2025-01`)
- Monthly partitioning prevents hot partitions
- Category prefix enables efficient sorting

### Simplicity

Table Storage requires:
- ✅ No schema migrations
- ✅ No connection pooling
- ✅ No ORM configuration
- ✅ Built-in retry policies
- ✅ SDK support via `Azure.Data.Tables`

### Scalability

- **Partition throughput**: 2,000 entities/second (far exceeds our needs)
- **Table throughput**: 20,000 entities/second
- **Storage**: 500 TB per account
- **Auto-scaling**: No configuration needed

## Alternatives Considered

### Azure SQL Database

**Pros**:
- Familiar relational model
- Complex queries with JOINs
- Stored procedures

**Cons**:
- Higher cost for simple key-value access
- Connection pooling complexity
- Schema migration overhead
- Overkill for our simple data model

### Azure Cosmos DB

**Pros**:
- Global distribution
- Multiple consistency levels
- Rich query language

**Cons**:
- Minimum 400 RU/s = ~$25/month
- Complex pricing model
- Unnecessary features for single-region app

### Blob Storage with JSON

**Pros**:
- Cheapest storage option
- Simple file-based model

**Cons**:
- No indexing
- Query requires full blob download
- Poor leaderboard performance
- No atomic updates

## Consequences

### Positive

1. **Cost**: Monthly storage costs will be under $1 for typical usage
2. **Operations**: Zero database maintenance required
3. **Performance**: Sub-10ms reads for point queries
4. **Development**: Simple SDK, no ORM complexity

### Negative

1. **Query Limitations**: No complex JOINs or aggregations
2. **Transactions**: Limited to single-partition batch operations
3. **Indexing**: Only PartitionKey + RowKey indexed; property queries require scans

### Mitigations

1. **Query Limitations**: Design partitions for access patterns; aggregate in memory for small result sets
2. **Transactions**: Structure data to keep related entities in same partition
3. **Indexing**: Use PartitionKey prefixes for filtering; limit property queries to small partitions

## Implementation Notes

### Connection String Management

```csharp
// Development: Azurite local emulator
"UseDevelopmentStorage=true"

// Production: Managed Identity
new TableServiceClient(
    new Uri("https://{account}.table.core.windows.net"),
    new DefaultAzureCredential());
```

### Entity Mapping

```csharp
public class JokePerformanceEntity : ITableEntity
{
    public string PartitionKey { get; set; }  // "{Category}_{Month}"
    public string RowKey { get; set; }        // "{InvertedTimestamp}_{Id}"
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    // Custom properties
    public string Setup { get; set; }
    public string Delivery { get; set; }
    public string AiPrediction { get; set; }
    public bool WasCorrect { get; set; }
    public int Cleverness { get; set; }
    public int Rudeness { get; set; }
    public int Complexity { get; set; }
    public int Difficulty { get; set; }
    public string Commentary { get; set; }
}
```

### Inverted Timestamp Pattern

```csharp
// For "latest first" ordering by RowKey
var invertedTicks = DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks;
var rowKey = $"{invertedTicks:D19}_{jokeId}";
```

## References

- [Azure Table Storage Documentation](https://learn.microsoft.com/en-us/azure/storage/tables/)
- [Table Storage Design Guide](https://learn.microsoft.com/en-us/azure/storage/tables/table-storage-design-guide)
- [Azure.Data.Tables SDK](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/data.tables-readme)
