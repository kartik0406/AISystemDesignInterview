using SdiApiGateway.Models.McpSchemas;
using SdiApiGateway.Services;
using SdiApiGateway.Tools;

namespace SdiApiGateway.Agents;

/// <summary>
/// Specialized agent for providing progressive hints.
/// Gives partial guidance without revealing full solutions.
/// Ported from Java HintAgent.
/// </summary>
public class HintAgent
{
    private readonly HintTool _hintTool;
    private readonly RagTool _ragTool;
    private readonly SessionService _sessionService;
    private readonly ILogger<HintAgent> _logger;
    private readonly AgentCard _card = AgentCard.HintAgent();

    public AgentCard Card => _card;

    public HintAgent(HintTool hintTool, RagTool ragTool,
        SessionService sessionService, ILogger<HintAgent> logger)
    {
        _hintTool = hintTool;
        _ragTool = ragTool;
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task<string> GenerateHintAsync(Guid sessionId, string currentQuestion, int hintLevel)
    {
        _logger.LogInformation("[HintAgent] Generating hint level={Level} for session={SessionId}",
            hintLevel, sessionId);

        var history = _sessionService.GetRecentHistory(sessionId, 2);
        var ragResult = await _ragTool.QueryAsync(new RagQueryRequest { Query = currentQuestion, TopK = 3 });

        var request = new HintMcpRequest
        {
            Question = currentQuestion,
            HintLevel = hintLevel,
            ConversationHistory = history,
            RagContext = ragResult.Chunks
        };

        var result = await _hintTool.GenerateHintAsync(request);
        return result.Hint;
    }
}
