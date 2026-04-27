using System.Text.Json;
using StackExchange.Redis;

namespace SdiApiGateway.Services;

/// <summary>
/// Redis-backed session memory service.
/// Stores conversation history, context, and interview state for follow-up generation.
/// Replaces the Spring Boot RedisTemplate-based SessionService.
/// </summary>
public class SessionService
{
    private readonly IDatabase _redis;
    private readonly ILogger<SessionService> _logger;
    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(30);
    private const string SessionPrefix = "sdi:session:";

    public SessionService(IConnectionMultiplexer redis, ILogger<SessionService> logger)
    {
        _redis = redis.GetDatabase();
        _logger = logger;
    }

    // ─── Conversation History ────────────────────────────────

    public void AddToHistory(Guid sessionId, string role, string content)
    {
        var key = $"{SessionPrefix}{sessionId}:history";
        var entry = new Dictionary<string, string>
        {
            { "role", role },
            { "content", content },
            { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }
        };

        try
        {
            var json = JsonSerializer.Serialize(entry);
            _redis.ListRightPush(key, json);
            _redis.KeyExpire(key, SessionTtl);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to add history entry: {Message}", ex.Message);
        }
    }

    public List<Dictionary<string, string>> GetRecentHistory(Guid sessionId, int lastN)
    {
        var key = $"{SessionPrefix}{sessionId}:history";
        var history = new List<Dictionary<string, string>>();

        try
        {
            var size = _redis.ListLength(key);
            if (size == 0) return history;

            var start = Math.Max(0, size - lastN * 2); // each exchange is 2 entries (Q + A)
            var raw = _redis.ListRange(key, start, -1);

            foreach (var item in raw)
            {
                var entry = JsonSerializer.Deserialize<Dictionary<string, string>>(item.ToString());
                if (entry != null) history.Add(entry);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to get history: {Message}", ex.Message);
        }

        return history;
    }

    // ─── Previous Questions Tracking ────────────────────────

    public void AddPreviousQuestion(Guid sessionId, string question)
    {
        var key = $"{SessionPrefix}{sessionId}:questions";
        try
        {
            _redis.ListRightPush(key, question);
            _redis.KeyExpire(key, SessionTtl);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to add previous question: {Message}", ex.Message);
        }
    }

    public List<string> GetPreviousQuestions(Guid sessionId)
    {
        var key = $"{SessionPrefix}{sessionId}:questions";
        try
        {
            var raw = _redis.ListRange(key, 0, -1);
            return raw.Select(r => r.ToString()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to get previous questions: {Message}", ex.Message);
            return new List<string>();
        }
    }

    // ─── Session Metadata ────────────────────────────────────

    public void SetSessionMeta(Guid sessionId, string field, string value)
    {
        var key = $"{SessionPrefix}{sessionId}:meta";
        try
        {
            _redis.HashSet(key, field, value);
            _redis.KeyExpire(key, SessionTtl);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to set session meta: {Message}", ex.Message);
        }
    }

    public string? GetSessionMeta(Guid sessionId, string field)
    {
        var key = $"{SessionPrefix}{sessionId}:meta";
        try
        {
            var value = _redis.HashGet(key, field);
            return value.HasValue ? value.ToString() : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to get session meta: {Message}", ex.Message);
            return null;
        }
    }
}
