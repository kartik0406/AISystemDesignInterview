using System.Text;
using System.Text.Json;
using SdiApiGateway.Config;
using SdiApiGateway.Llm;
using Microsoft.Extensions.Options;

namespace SdiApiGateway.Rag;

/// <summary>
/// Pinecone-based retriever for RAG (Retrieval-Augmented Generation).
/// Uses Gemini Embeddings + Pinecone REST API.
/// Replaces both Python embeddings.py and retriever.py.
/// </summary>
public class PineconeRetriever
{
    private readonly HttpClient _httpClient;
    private readonly GeminiClient _geminiClient;
    private readonly AppSettings _settings;
    private readonly ILogger<PineconeRetriever> _logger;
    private string? _cachedHost;
    private readonly SemaphoreSlim _hostSemaphore = new(1, 1);

    public PineconeRetriever(HttpClient httpClient, GeminiClient geminiClient,
        IOptions<AppSettings> settings, ILogger<PineconeRetriever> logger)
    {
        _httpClient = httpClient;
        _geminiClient = geminiClient;
        _settings = settings.Value;
        _logger = logger;
        _cachedHost = string.IsNullOrEmpty(settings.Value.Pinecone.Host) ? null : settings.Value.Pinecone.Host;
    }

    /// <summary>Resolve the Pinecone host dynamically via the Control Plane API.</summary>
    private async Task<string?> GetHostAsync()
    {
        if (!string.IsNullOrEmpty(_cachedHost)) return _cachedHost;

        await _hostSemaphore.WaitAsync();
        try
        {
            if (!string.IsNullOrEmpty(_cachedHost)) return _cachedHost;

            var indexName = _settings.Pinecone.IndexName;
            var apiKey = _settings.Pinecone.ApiKey;

            if (string.IsNullOrEmpty(indexName) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Pinecone ApiKey or IndexName missing, cannot resolve host");
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.pinecone.io/indexes/{indexName}");
            request.Headers.Add("Api-Key", apiKey);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("host", out var hostElement))
                {
                    _cachedHost = hostElement.GetString();
                    _logger.LogInformation("Resolved Pinecone host: {Host}", _cachedHost);
                    return _cachedHost;
                }
            }
            else
            {
                _logger.LogError("Failed to resolve Pinecone host: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving Pinecone host");
        }
        finally
        {
            _hostSemaphore.Release();
        }

        return null;
    }

    /// <summary>Query Pinecone for the most relevant chunks.</summary>
    public async Task<List<Dictionary<string, object>>> QueryAsync(string queryText, int topK = 5)
    {
        var results = new List<Dictionary<string, object>>();

        try
        {
            // Resolve host
            var host = await GetHostAsync();
            if (string.IsNullOrEmpty(host))
            {
                _logger.LogWarning("Pinecone host not configured/resolved, skipping vector search");
                return results;
            }

            // Get embedding from Gemini
            var queryVector = await _geminiClient.EmbedAsync(queryText);

            // Query Pinecone REST API
            var url = $"https://{host}/query";

            var payload = new
            {
                vector = queryVector,
                topK,
                includeMetadata = true
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = content;
            request.Headers.Add("Api-Key", _settings.Pinecone.ApiKey);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseBody);
                var matches = doc.RootElement.GetProperty("matches");

                foreach (var match in matches.EnumerateArray())
                {
                    var metadata = match.GetProperty("metadata");
                    results.Add(new Dictionary<string, object>
                    {
                        ["text"] = metadata.TryGetProperty("text", out var t) ? t.GetString() ?? "" : "",
                        ["source"] = metadata.TryGetProperty("source", out var s) ? s.GetString() ?? "unknown" : "unknown",
                        ["score"] = match.GetProperty("score").GetDouble()
                    });
                }
            }
            else
            {
                _logger.LogError("Pinecone query failed: {StatusCode} {Body}", response.StatusCode, responseBody[..Math.Min(200, responseBody.Length)]);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pinecone query failed");
        }

        return results;
    }
}
