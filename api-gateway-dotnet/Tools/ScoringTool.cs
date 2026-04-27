using System.Text.Json;
using SdiApiGateway.Llm;
using SdiApiGateway.Models.McpSchemas;

namespace SdiApiGateway.Tools;

/// <summary>
/// MCP Tool: Evaluates candidate answers with structured rubric scoring.
/// Ported from Python tools/scoring_tool.py.
/// </summary>
public class ScoringTool
{
    private readonly GeminiClient _geminiClient;
    private readonly ILogger<ScoringTool> _logger;

    public ScoringTool(GeminiClient geminiClient, ILogger<ScoringTool> logger)
    {
        _geminiClient = geminiClient;
        _logger = logger;
    }

    public async Task<ScoreResponse> ScoreAsync(ScoreRequest request)
    {
        try
        {
            var historyText = request.ConversationHistory.Count > 0
                ? string.Join("\n", request.ConversationHistory.TakeLast(6)
                    .Select(e => $"{e.GetValueOrDefault("role", "unknown")}: {e.GetValueOrDefault("content", "")}"))
                : "No previous conversation.";

            var ragText = request.RagContext.Count > 0
                ? string.Join("\n---\n", request.RagContext.Take(5))
                : "No reference context available.";

            var weightsText = request.RubricWeights.Count > 0
                ? JsonSerializer.Serialize(request.RubricWeights)
                : "Equal weights (2.0 each)";

            var prompt = string.Format(Prompts.Evaluation,
                request.CompanyMode, request.Question, request.Answer,
                ragText, historyText, weightsText);

            var result = await _geminiClient.GenerateJsonAsync(prompt);

            return new ScoreResponse
            {
                Score = GetDouble(result, "score", 5.0),
                MaxScore = GetDouble(result, "maxScore", 10.0),
                Strengths = GetStringList(result, "strengths"),
                Weaknesses = GetStringList(result, "weaknesses"),
                Suggestions = GetStringList(result, "suggestions"),
                RubricBreakdown = GetDoubleDictionary(result, "rubricBreakdown"),
                DifficultyAdjustment = GetString(result, "difficultyAdjustment", "maintain")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scoring failed");
            return new ScoreResponse
            {
                Score = 5.0,
                MaxScore = 10.0,
                Strengths = new() { "Answer received and processed" },
                Weaknesses = new() { "Evaluation encountered an issue — please try again" },
                Suggestions = new() { "Ensure your answer covers the key system design aspects" },
                RubricBreakdown = new()
                {
                    ["scalability"] = 5.0, ["database_design"] = 5.0,
                    ["api_design"] = 5.0, ["tradeoffs"] = 5.0, ["clarity"] = 5.0
                },
                DifficultyAdjustment = "maintain"
            };
        }
    }

    private static double GetDouble(Dictionary<string, object> dict, string key, double defaultVal)
    {
        if (dict.TryGetValue(key, out var val) && val is JsonElement el && el.ValueKind == JsonValueKind.Number)
            return el.GetDouble();
        return defaultVal;
    }

    private static string GetString(Dictionary<string, object> dict, string key, string defaultVal)
    {
        if (dict.TryGetValue(key, out var val) && val is JsonElement el && el.ValueKind == JsonValueKind.String)
            return el.GetString() ?? defaultVal;
        return defaultVal;
    }

    private static List<string> GetStringList(Dictionary<string, object> dict, string key)
    {
        if (dict.TryGetValue(key, out var val) && val is JsonElement el && el.ValueKind == JsonValueKind.Array)
            return el.EnumerateArray().Select(e => e.GetString() ?? "").ToList();
        return new List<string>();
    }

    private static Dictionary<string, double> GetDoubleDictionary(Dictionary<string, object> dict, string key)
    {
        if (dict.TryGetValue(key, out var val) && val is JsonElement el && el.ValueKind == JsonValueKind.Object)
        {
            var result = new Dictionary<string, double>();
            foreach (var prop in el.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Number)
                    result[prop.Name] = prop.Value.GetDouble();
            }
            return result;
        }
        return new Dictionary<string, double>();
    }
}
