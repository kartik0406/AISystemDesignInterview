using SdiApiGateway.Models.DTOs;
using SdiApiGateway.Models.Enums;

namespace SdiApiGateway.Agents;

/// <summary>
/// Interview Orchestrator Agent — the brain of the system.
/// Routes requests to specialized agents and manages interview state machine.
/// Ported from Java InterviewAgent.
/// </summary>
public class InterviewAgent
{
    private readonly QuestionAgent _questionAgent;
    private readonly EvaluationAgent _evaluationAgent;
    private readonly HintAgent _hintAgent;
    private readonly ILogger<InterviewAgent> _logger;

    public InterviewAgent(QuestionAgent questionAgent, EvaluationAgent evaluationAgent,
        HintAgent hintAgent, ILogger<InterviewAgent> logger)
    {
        _questionAgent = questionAgent;
        _evaluationAgent = evaluationAgent;
        _hintAgent = hintAgent;
        _logger = logger;
    }

    public async Task<Dictionary<string, object>> RouteToQuestionAgentAsync(
        Guid sessionId, string topic, CompanyMode companyMode,
        DifficultyLevel difficulty, List<Dictionary<string, string>> history)
    {
        _logger.LogInformation("[InterviewAgent] Routing to QuestionAgent for session={SessionId}", sessionId);
        return await _questionAgent.GenerateQuestionAsync(sessionId, topic, companyMode, difficulty, history);
    }

    public async Task<EvaluationResponse> RouteToEvaluationAgentAsync(
        Guid sessionId, string question, string answer, CompanyMode companyMode)
    {
        _logger.LogInformation("[InterviewAgent] Routing to EvaluationAgent for session={SessionId}", sessionId);
        return await _evaluationAgent.EvaluateAnswerAsync(sessionId, question, answer, companyMode);
    }

    public async Task<string> RouteToHintAgentAsync(Guid sessionId, string currentQuestion, int hintLevel)
    {
        _logger.LogInformation("[InterviewAgent] Routing to HintAgent for session={SessionId}, level={Level}", sessionId, hintLevel);
        return await _hintAgent.GenerateHintAsync(sessionId, currentQuestion, hintLevel);
    }

    public List<AgentCard> DiscoverAgents()
    {
        return new List<AgentCard>
        {
            _questionAgent.Card,
            _evaluationAgent.Card,
            _hintAgent.Card
        };
    }
}
