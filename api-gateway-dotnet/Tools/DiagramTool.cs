using SdiApiGateway.Llm;
using SdiApiGateway.Models.McpSchemas;

namespace SdiApiGateway.Tools;

/// <summary>
/// MCP Tool: Generates Mermaid architecture diagrams from system descriptions.
/// Ported from Python tools/diagram_tool.py.
/// </summary>
public class DiagramTool
{
    private readonly GeminiClient _geminiClient;
    private readonly ILogger<DiagramTool> _logger;

    public DiagramTool(GeminiClient geminiClient, ILogger<DiagramTool> logger)
    {
        _geminiClient = geminiClient;
        _logger = logger;
    }

    public async Task<DiagramResponse> GenerateAsync(DiagramRequest request)
    {
        try
        {
            var components = request.Components.Count > 0
                ? string.Join(", ", request.Components)
                : "standard web architecture components";

            var prompt = string.Format(Prompts.Diagram, request.SystemDescription, components);

            var result = await _geminiClient.GenerateAsync(prompt, 0.3);

            var diagram = result.Trim();
            if (diagram.StartsWith("```mermaid"))
                diagram = diagram["```mermaid".Length..].Trim();
            if (diagram.StartsWith("```"))
                diagram = diagram[3..].Trim();
            if (diagram.EndsWith("```"))
                diagram = diagram[..^3].Trim();

            // Validate it starts with a valid Mermaid keyword
            var validStarts = new[] { "graph ", "flowchart ", "sequenceDiagram", "classDiagram", "erDiagram" };
            if (!validStarts.Any(s => diagram.StartsWith(s)))
                diagram = "graph TB\n    Client[Client] --> LB[Load Balancer]\n    LB --> API[API Server]\n    API --> DB[(Database)]";

            _logger.LogInformation("Generated diagram for: {Desc}", request.SystemDescription[..Math.Min(50, request.SystemDescription.Length)]);
            return new DiagramResponse { Diagram = diagram };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Diagram generation failed");
            return new DiagramResponse
            {
                Diagram = "graph TB\n    Client[Client] --> API[API Gateway]\n    API --> Service[Service]\n    Service --> DB[(Database)]"
            };
        }
    }
}
