namespace SdiApiGateway.Models.McpSchemas;

// ─── RAG ──────────────────────────────────────────────────────

public class RagQueryRequest
{
    public string Query { get; set; } = string.Empty;
    public int TopK { get; set; } = 5;
}

public class RagQueryResponse
{
    public List<string> Chunks { get; set; } = new();
    public List<string> Sources { get; set; } = new();
    public List<double> Scores { get; set; } = new();
}

// ─── Question Generation ─────────────────────────────────────

public class QuestionRequest
{
    public string Topic { get; set; } = string.Empty;
    public string CompanyMode { get; set; } = "GENERAL";
    public int Difficulty { get; set; } = 5;
    public List<string> PreviousQuestions { get; set; } = new();
    public List<Dictionary<string, string>> ConversationHistory { get; set; } = new();
    public List<string> RagContext { get; set; } = new();
    public List<string> FocusAreas { get; set; } = new();
}

public class QuestionResponse
{
    public string Question { get; set; } = string.Empty;
    public string TopicArea { get; set; } = string.Empty;
    public string ExpectedDepth { get; set; } = string.Empty;
}

// ─── Scoring / Evaluation ────────────────────────────────────

public class ScoreRequest
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string CompanyMode { get; set; } = "GENERAL";
    public List<string> RagContext { get; set; } = new();
    public List<Dictionary<string, string>> ConversationHistory { get; set; } = new();
    public Dictionary<string, double> RubricWeights { get; set; } = new();
}

public class ScoreResponse
{
    public double Score { get; set; }
    public double MaxScore { get; set; } = 10.0;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public Dictionary<string, double> RubricBreakdown { get; set; } = new();
    public string DifficultyAdjustment { get; set; } = "maintain";
}

// ─── Diagram ─────────────────────────────────────────────────

public class DiagramRequest
{
    public string SystemDescription { get; set; } = string.Empty;
    public List<string> Components { get; set; } = new();
}

public class DiagramResponse
{
    public string Diagram { get; set; } = string.Empty;
}

// ─── Hint ────────────────────────────────────────────────────

public class HintMcpRequest
{
    public string Question { get; set; } = string.Empty;
    public int HintLevel { get; set; } = 1;
    public List<Dictionary<string, string>> ConversationHistory { get; set; } = new();
    public List<string> RagContext { get; set; } = new();
}

public class HintResponse
{
    public string Hint { get; set; } = string.Empty;
    public int Level { get; set; }
}

// ─── MCP Tool Manifest ───────────────────────────────────────

public class ToolDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = "POST";
    public Dictionary<string, string> Parameters { get; set; } = new();
}

public class ToolManifest
{
    public List<ToolDefinition> Tools { get; set; } = new();
    public string Version { get; set; } = "1.0.0";
}
