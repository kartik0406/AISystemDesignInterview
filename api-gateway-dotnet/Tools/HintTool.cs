using SdiApiGateway.Llm;
using SdiApiGateway.Models.McpSchemas;

namespace SdiApiGateway.Tools;

/// <summary>
/// MCP Tool: Generates progressive hints (nudge → direction → partial solution).
/// Ported from Python tools/hint_tool.py.
/// </summary>
public class HintTool
{
    private readonly GeminiClient _geminiClient;
    private readonly ILogger<HintTool> _logger;

    public HintTool(GeminiClient geminiClient, ILogger<HintTool> logger)
    {
        _geminiClient = geminiClient;
        _logger = logger;
    }

    public async Task<HintResponse> GenerateHintAsync(HintMcpRequest request)
    {
        try
        {
            var level = Math.Clamp(request.HintLevel, 1, 3);

            var historyText = request.ConversationHistory.Count > 0
                ? string.Join("\n", request.ConversationHistory.TakeLast(4)
                    .Select(e => $"{e.GetValueOrDefault("role", "unknown")}: {e.GetValueOrDefault("content", "")}"))
                : "No previous conversation.";

            var ragText = request.RagContext.Count > 0
                ? string.Join("\n---\n", request.RagContext.Take(3))
                : "";

            var promptTemplate = Prompts.Hints[level];
            var prompt = string.Format(promptTemplate, request.Question, historyText, ragText);

            var hint = await _geminiClient.GenerateAsync(prompt, 0.6);

            _logger.LogInformation("Generated level-{Level} hint for: {Question}",
                level, request.Question[..Math.Min(50, request.Question.Length)]);

            return new HintResponse { Hint = hint.Trim(), Level = level };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hint generation failed");
            var fallbackHints = new Dictionary<int, string>
            {
                [1] = "Think about what happens as the number of users grows significantly.",
                [2] = "Consider how you would handle data consistency across multiple services. What patterns exist for this?",
                [3] = "You might want to consider using a message queue (like Kafka) for async processing, and a cache layer (like Redis) in front of your database to reduce read latency."
            };

            return new HintResponse
            {
                Hint = fallbackHints.GetValueOrDefault(request.HintLevel, fallbackHints[1]),
                Level = request.HintLevel
            };
        }
    }
}
