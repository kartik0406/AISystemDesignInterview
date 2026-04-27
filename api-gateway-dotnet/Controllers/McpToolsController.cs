using Microsoft.AspNetCore.Mvc;
using SdiApiGateway.Llm;
using SdiApiGateway.Models.McpSchemas;
using SdiApiGateway.Tools;

namespace SdiApiGateway.Controllers;

/// <summary>
/// MCP Tools Controller — exposes all MCP tool endpoints.
/// Ported from Python routers/mcp_tools.py.
/// </summary>
[ApiController]
[Route("tools")]
public class McpToolsController : ControllerBase
{
    private readonly RagTool _ragTool;
    private readonly ScoringTool _scoringTool;
    private readonly DiagramTool _diagramTool;
    private readonly HintTool _hintTool;
    private readonly GeminiClient _geminiClient;
    private readonly ILogger<McpToolsController> _logger;

    public McpToolsController(RagTool ragTool, ScoringTool scoringTool,
        DiagramTool diagramTool, HintTool hintTool, GeminiClient geminiClient,
        ILogger<McpToolsController> logger)
    {
        _ragTool = ragTool;
        _scoringTool = scoringTool;
        _diagramTool = diagramTool;
        _hintTool = hintTool;
        _geminiClient = geminiClient;
        _logger = logger;
    }

    /// <summary>MCP tool discovery endpoint. Lists all available tools.</summary>
    [HttpGet("manifest")]
    public IActionResult GetManifest()
    {
        return Ok(new ToolManifest
        {
            Tools = new List<ToolDefinition>
            {
                new() { Name = "rag_query", Description = "Retrieve relevant system design knowledge from vector database", Endpoint = "/tools/rag/query", Parameters = new() { ["query"] = "string", ["top_k"] = "int (1-20)" } },
                new() { Name = "generate_question", Description = "Generate an adaptive interview question based on context", Endpoint = "/tools/generate-question", Parameters = new() { ["topic"] = "string", ["difficulty"] = "int (1-10)", ["company_mode"] = "string" } },
                new() { Name = "score", Description = "Evaluate a candidate answer with structured rubric scoring", Endpoint = "/tools/score", Parameters = new() { ["question"] = "string", ["answer"] = "string", ["company_mode"] = "string" } },
                new() { Name = "diagram", Description = "Generate a Mermaid architecture diagram", Endpoint = "/tools/diagram", Parameters = new() { ["system_description"] = "string" } },
                new() { Name = "hint", Description = "Generate a progressive hint (levels 1-3)", Endpoint = "/tools/hint", Parameters = new() { ["question"] = "string", ["hint_level"] = "int (1-3)" } }
            },
            Version = "1.0.0"
        });
    }

    [HttpPost("rag/query")]
    public async Task<IActionResult> RagQuery([FromBody] RagQueryRequest request)
    {
        var result = await _ragTool.QueryAsync(request);
        return Ok(result);
    }

    [HttpPost("generate-question")]
    public async Task<IActionResult> GenerateQuestion([FromBody] QuestionRequest request)
    {
        try
        {
            var historyText = request.ConversationHistory.Count > 0
                ? string.Join("\n", request.ConversationHistory.TakeLast(6)
                    .Select(e => $"{e.GetValueOrDefault("role", "unknown")}: {e.GetValueOrDefault("content", "")}"))
                : "This is the first question.";

            var prevQText = request.PreviousQuestions.Count > 0
                ? string.Join("\n", request.PreviousQuestions.Select(q => $"- {q}"))
                : "None yet.";

            var ragText = request.RagContext.Count > 0
                ? string.Join("\n---\n", request.RagContext.Take(3))
                : "No reference context.";

            var focusText = request.FocusAreas.Count > 0
                ? string.Join(", ", request.FocusAreas)
                : "general system design";

            var prompt = string.Format(Prompts.QuestionGenerator,
                request.CompanyMode, request.Topic, request.Difficulty,
                prevQText, historyText, ragText, focusText);

            var result = await _geminiClient.GenerateJsonAsync(prompt);

            return Ok(new QuestionResponse
            {
                Question = result.TryGetValue("question", out var q)
                    ? q?.ToString() ?? $"Tell me about how you would design {request.Topic}."
                    : $"Tell me about how you would design {request.Topic}.",
                TopicArea = result.TryGetValue("topic_area", out var ta) ? ta?.ToString() ?? "general" : "general",
                ExpectedDepth = result.TryGetValue("expected_depth", out var ed) ? ed?.ToString() ?? "overview" : "overview"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Question generation failed");
            return Ok(new QuestionResponse
            {
                Question = $"Walk me through the high-level architecture for {request.Topic}. What are the main components?",
                TopicArea = "high-level design",
                ExpectedDepth = "overview"
            });
        }
    }

    [HttpPost("score")]
    public async Task<IActionResult> ScoreAnswer([FromBody] ScoreRequest request)
    {
        var result = await _scoringTool.ScoreAsync(request);
        return Ok(result);
    }

    [HttpPost("diagram")]
    public async Task<IActionResult> GenerateDiagram([FromBody] DiagramRequest request)
    {
        var result = await _diagramTool.GenerateAsync(request);
        return Ok(result);
    }

    [HttpPost("hint")]
    public async Task<IActionResult> GenerateHint([FromBody] HintMcpRequest request)
    {
        var result = await _hintTool.GenerateHintAsync(request);
        return Ok(result);
    }
}
