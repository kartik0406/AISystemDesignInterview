using System.Text.Json;
using SdiApiGateway.Llm;
using SdiApiGateway.Models.Enums;
using SdiApiGateway.Services;
using SdiApiGateway.Tools;

namespace SdiApiGateway.Agents;

/// <summary>
/// Specialized agent for generating adaptive interview questions.
/// Uses RAG context and conversation history to produce relevant follow-ups.
/// Ported from Java QuestionAgent.
/// </summary>
public class QuestionAgent
{
    private readonly GeminiClient _geminiClient;
    private readonly RagTool _ragTool;
    private readonly SessionService _sessionService;
    private readonly ILogger<QuestionAgent> _logger;
    private readonly AgentCard _card = AgentCard.QuestionAgent();

    public AgentCard Card => _card;

    public QuestionAgent(GeminiClient geminiClient, RagTool ragTool,
        SessionService sessionService, ILogger<QuestionAgent> logger)
    {
        _geminiClient = geminiClient;
        _ragTool = ragTool;
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task<Dictionary<string, object>> GenerateQuestionAsync(
        Guid sessionId, string topic, CompanyMode companyMode,
        DifficultyLevel difficulty, List<Dictionary<string, string>> recentHistory)
    {
        _logger.LogInformation("[QuestionAgent] Generating question for session={SessionId}, topic={Topic}, difficulty={Difficulty}",
            sessionId, topic, difficulty);

        // Get previous questions to avoid repetition
        var previousQuestions = _sessionService.GetPreviousQuestions(sessionId);

        // Fetch relevant knowledge via RAG
        var ragResult = await _ragTool.QueryAsync(new() { Query = topic, TopK = 3 });
        var ragContext = ragResult.Chunks;

        // Company-specific focus areas
        var focusAreas = companyMode switch
        {
            CompanyMode.GOOGLE => new List<string> { "scalability", "distributed systems", "data consistency", "fault tolerance" },
            CompanyMode.AMAZON => new List<string> { "trade-offs", "cost optimization", "customer impact", "operational excellence" },
            _ => new List<string> { "scalability", "database design", "API design", "trade-offs", "caching" }
        };

        // Format conversation history
        var historyText = recentHistory.Count > 0
            ? string.Join("\n", recentHistory.TakeLast(6)
                .Select(e => $"{e.GetValueOrDefault("role", "unknown")}: {e.GetValueOrDefault("content", "")}"))
            : "This is the first question.";

        var prevQText = previousQuestions.Count > 0
            ? string.Join("\n", previousQuestions.Select(q => $"- {q}"))
            : "None yet.";

        var ragText = ragContext.Count > 0
            ? string.Join("\n---\n", ragContext.Take(3))
            : "No reference context.";

        var focusText = string.Join(", ", focusAreas);

        var prompt = string.Format(Prompts.QuestionGenerator,
            companyMode.ToString(), topic, (int)difficulty,
            prevQText, historyText, ragText, focusText);

        try
        {
            var result = await _geminiClient.GenerateJsonAsync(prompt);

            var question = result.TryGetValue("question", out var q) && q is JsonElement qEl
                ? qEl.GetString() ?? $"Tell me about how you would design {topic}."
                : $"Tell me about how you would design {topic}.";

            var topicArea = result.TryGetValue("topic_area", out var ta) && ta is JsonElement taEl
                ? taEl.GetString() ?? "general"
                : "general";

            var expectedDepth = result.TryGetValue("expected_depth", out var ed) && ed is JsonElement edEl
                ? edEl.GetString() ?? "overview"
                : "overview";

            // Track this question
            _sessionService.AddPreviousQuestion(sessionId, question);
            _sessionService.AddToHistory(sessionId, "interviewer", question);

            return new Dictionary<string, object>
            {
                ["question"] = question,
                ["topic_area"] = topicArea,
                ["expected_depth"] = expectedDepth
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Question generation failed");
            var fallbackQ = $"Walk me through the high-level architecture for {topic}. What are the main components?";
            _sessionService.AddPreviousQuestion(sessionId, fallbackQ);
            _sessionService.AddToHistory(sessionId, "interviewer", fallbackQ);

            return new Dictionary<string, object>
            {
                ["question"] = fallbackQ,
                ["topic_area"] = "high-level design",
                ["expected_depth"] = "overview"
            };
        }
    }
}
