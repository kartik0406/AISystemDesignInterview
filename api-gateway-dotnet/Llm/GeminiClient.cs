using System.Text;
using System.Text.Json;
using SdiApiGateway.Config;
using Microsoft.Extensions.Options;

namespace SdiApiGateway.Llm;

/// <summary>
/// Google Gemini client wrapper using the REST API.
/// Handles all LLM interactions with structured output parsing.
/// Replaces the Python google.genai client.
/// </summary>
public class GeminiClient
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _settings;
    private readonly ILogger<GeminiClient> _logger;
    private readonly string _baseUrl;

    public GeminiClient(HttpClient httpClient, IOptions<AppSettings> settings, ILogger<GeminiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _baseUrl = "https://generativelanguage.googleapis.com/v1beta";
    }

    /// <summary>Generate text from a prompt.</summary>
    public async Task<string> GenerateAsync(string prompt, double temperature = 0.7)
    {
        try
        {
            var url = $"{_baseUrl}/models/{_settings.Gemini.Model}:generateContent?key={_settings.Gemini.ApiKey}";

            var payload = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new
                {
                    temperature,
                    maxOutputTokens = 2048
                }
            };

            var response = await PostJsonAsync(url, payload);
            return ExtractText(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini generation failed");
            throw;
        }
    }

    /// <summary>Generate and parse JSON from a prompt.</summary>
    public async Task<Dictionary<string, object>> GenerateJsonAsync(string prompt, double temperature = 0.4)
    {
        try
        {
            var url = $"{_baseUrl}/models/{_settings.Gemini.Model}:generateContent?key={_settings.Gemini.ApiKey}";

            var payload = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new
                {
                    temperature,
                    maxOutputTokens = 2048,
                    responseMimeType = "application/json"
                }
            };

            var response = await PostJsonAsync(url, payload);
            var text = ExtractText(response).Trim();

            // Clean up potential markdown code fences
            if (text.StartsWith("```"))
            {
                var lines = text.Split('\n');
                text = string.Join('\n', lines.Skip(1));
                if (text.EndsWith("```"))
                    text = text[..^3].Trim();
            }

            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(text);
            return result ?? new Dictionary<string, object>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON from Gemini response");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini JSON generation failed");
            throw;
        }
    }

    /// <summary>Generate embeddings using Gemini Embeddings API.</summary>
    public async Task<float[]> EmbedAsync(string text)
    {
        try
        {
            var url = $"{_baseUrl}/models/{_settings.Embedding.Model}:embedContent?key={_settings.Gemini.ApiKey}";

            var payload = new
            {
                content = new { parts = new[] { new { text } } },
                outputDimensionality = _settings.Embedding.Dimension
            };

            var response = await PostJsonAsync(url, payload);
            return ExtractEmbedding(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini embedding failed");
            return new float[_settings.Embedding.Dimension];
        }
    }

    private async Task<JsonDocument> PostJsonAsync(string url, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API error {StatusCode}: {Body}", response.StatusCode, responseBody[..Math.Min(500, responseBody.Length)]);
            throw new HttpRequestException($"Gemini API returned {response.StatusCode}");
        }

        return JsonDocument.Parse(responseBody);
    }

    private static string ExtractText(JsonDocument doc)
    {
        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }

    private static float[] ExtractEmbedding(JsonDocument doc)
    {
        var values = doc.RootElement
            .GetProperty("embedding")
            .GetProperty("values");

        var result = new float[values.GetArrayLength()];
        int i = 0;
        foreach (var val in values.EnumerateArray())
        {
            result[i++] = val.GetSingle();
        }
        return result;
    }
}
