using SdiApiGateway.Models.McpSchemas;
using SdiApiGateway.Rag;

namespace SdiApiGateway.Tools;

/// <summary>
/// MCP Tool: Retrieves relevant knowledge using Pinecone vector search with fallback.
/// Ported from Python tools/rag_tool.py.
/// </summary>
public class RagTool
{
    private readonly PineconeRetriever _retriever;
    private readonly ILogger<RagTool> _logger;

    // Hardcoded fallback knowledge map
    private static readonly Dictionary<string, List<string>> FallbackKnowledge = new()
    {
        ["load balancing"] = new() { "Load balancing distributes traffic across servers using algorithms like round-robin, least connections, or consistent hashing. Key considerations: health checks, session stickiness, L4 vs L7 balancing." },
        ["caching"] = new() { "Caching strategies: Cache-aside (lazy loading), Write-through, Write-behind. Tools: Redis, Memcached. Consider cache invalidation (TTL, event-based), cache stampede prevention, and hot key handling." },
        ["database"] = new() { "Database design considerations: SQL vs NoSQL trade-offs, indexing strategies, replication (leader-follower, multi-leader), sharding (hash-based, range-based), ACID vs BASE properties." },
        ["microservices"] = new() { "Microservices patterns: API Gateway, Service Discovery, Circuit Breaker, Saga Pattern for distributed transactions, Event Sourcing, CQRS. Consider inter-service communication (sync REST/gRPC vs async messaging)." },
        ["scaling"] = new() { "Scaling strategies: Vertical scaling (bigger machines) vs Horizontal scaling (more machines). Stateless services for easy horizontal scaling. Use CDNs for static content. Database read replicas for read-heavy workloads." }
    };

    public RagTool(PineconeRetriever retriever, ILogger<RagTool> logger)
    {
        _retriever = retriever;
        _logger = logger;
    }

    public async Task<RagQueryResponse> QueryAsync(RagQueryRequest request)
    {
        try
        {
            var results = await _retriever.QueryAsync(request.Query, request.TopK);

            if (results.Count > 0)
            {
                return new RagQueryResponse
                {
                    Chunks = results.Select(r => r["text"].ToString()!).ToList(),
                    Sources = results.Select(r => r["source"].ToString()!).ToList(),
                    Scores = results.Select(r => Convert.ToDouble(r["score"])).ToList()
                };
            }

            _logger.LogWarning("Pinecone returned no results, using fallback knowledge");
            return FallbackQuery(request.Query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RAG query failed, falling back to hardcoded knowledge");
            return FallbackQuery(request.Query);
        }
    }

    private RagQueryResponse FallbackQuery(string query)
    {
        var queryLower = query.ToLower();
        var chunks = new List<string>();
        var sources = new List<string>();

        foreach (var (keyword, knowledgeChunks) in FallbackKnowledge)
        {
            if (queryLower.Contains(keyword))
            {
                chunks.AddRange(knowledgeChunks);
                sources.AddRange(knowledgeChunks.Select(_ => $"fallback:{keyword}"));
            }
        }

        if (chunks.Count == 0)
        {
            foreach (var (keyword, knowledgeChunks) in FallbackKnowledge)
            {
                chunks.AddRange(knowledgeChunks);
                sources.AddRange(knowledgeChunks.Select(_ => $"fallback:{keyword}"));
            }
        }

        return new RagQueryResponse
        {
            Chunks = chunks.Take(5).ToList(),
            Sources = sources.Take(5).ToList(),
            Scores = Enumerable.Repeat(0.5, Math.Min(chunks.Count, 5)).ToList()
        };
    }
}
