using SdiApiGateway.Models.DTOs;
using SdiApiGateway.Models.Enums;
using SdiApiGateway.Services;
using SdiApiGateway.Tools;
using SdiApiGateway.Models.McpSchemas;

namespace SdiApiGateway.Agents;

/// <summary>
/// Specialized agent for evaluating candidate answers.
/// Uses RAG context for reference architectures and structured rubric scoring.
/// Ported from Java EvaluationAgent.
/// </summary>
public class EvaluationAgent
{
    private readonly ScoringTool _scoringTool;
    private readonly RagTool _ragTool;
    private readonly SessionService _sessionService;
    private readonly ILogger<EvaluationAgent> _logger;
    private readonly AgentCard _card = AgentCard.EvaluationAgent();

    public AgentCard Card => _card;

    public EvaluationAgent(ScoringTool scoringTool, RagTool ragTool,
        SessionService sessionService, ILogger<EvaluationAgent> logger)
    {
        _scoringTool = scoringTool;
        _ragTool = ragTool;
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task<EvaluationResponse> EvaluateAnswerAsync(
        Guid sessionId, string question, string answer, CompanyMode companyMode)
    {
        _logger.LogInformation("[EvaluationAgent] Evaluating answer for session={SessionId}", sessionId);

        // Fetch reference context via RAG
        var ragResult = await _ragTool.QueryAsync(new RagQueryRequest
        {
            Query = $"{question} {answer}",
            TopK = 5
        });

        // Get conversation history for context
        var history = _sessionService.GetRecentHistory(sessionId, 3);

        // Company-specific rubric weights
        var rubricWeights = companyMode switch
        {
            CompanyMode.GOOGLE => new Dictionary<string, double>
            {
                ["scalability"] = 2.5, ["database_design"] = 2.0,
                ["api_design"] = 1.5, ["tradeoffs"] = 2.0, ["clarity"] = 2.0
            },
            CompanyMode.AMAZON => new Dictionary<string, double>
            {
                ["scalability"] = 2.0, ["database_design"] = 1.5,
                ["api_design"] = 2.0, ["tradeoffs"] = 2.5, ["clarity"] = 2.0
            },
            _ => new Dictionary<string, double>
            {
                ["scalability"] = 2.0, ["database_design"] = 2.0,
                ["api_design"] = 2.0, ["tradeoffs"] = 2.0, ["clarity"] = 2.0
            }
        };

        // Record user answer in session history
        _sessionService.AddToHistory(sessionId, "candidate", answer);

        var scoreRequest = new ScoreRequest
        {
            Question = question,
            Answer = answer,
            CompanyMode = companyMode.ToString(),
            RagContext = ragResult.Chunks,
            ConversationHistory = history,
            RubricWeights = rubricWeights
        };

        var scoreResult = await _scoringTool.ScoreAsync(scoreRequest);

        return new EvaluationResponse
        {
            Score = scoreResult.Score,
            MaxScore = scoreResult.MaxScore,
            Strengths = scoreResult.Strengths,
            Weaknesses = scoreResult.Weaknesses,
            Suggestions = scoreResult.Suggestions,
            RubricBreakdown = scoreResult.RubricBreakdown,
            DifficultyAdjustment = scoreResult.DifficultyAdjustment
        };
    }
}
