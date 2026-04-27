using System.ComponentModel.DataAnnotations;
using SdiApiGateway.Models.Enums;

namespace SdiApiGateway.Models.DTOs;

public class StartInterviewRequest
{
    [Required(ErrorMessage = "Topic is required")]
    public string Topic { get; set; } = string.Empty;

    public CompanyMode CompanyMode { get; set; } = CompanyMode.GENERAL;
}

public class SubmitAnswerRequest
{
    [Required(ErrorMessage = "Session ID is required")]
    public Guid SessionId { get; set; }

    [Required(ErrorMessage = "Answer cannot be empty")]
    public string Answer { get; set; } = string.Empty;
}

public class HintRequest
{
    [Required(ErrorMessage = "Session ID is required")]
    public Guid SessionId { get; set; }

    public int HintLevel { get; set; } = 1; // 1 = nudge, 2 = direction, 3 = partial solution
}

public class InterviewResponse
{
    public Guid SessionId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public CompanyMode CompanyMode { get; set; }
    public int CurrentRound { get; set; }
    public int MaxRounds { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public SessionStatus Status { get; set; }
    public string? Question { get; set; }
    public string? TopicArea { get; set; }

    // Present only after answer submission
    public EvaluationResponse? Evaluation { get; set; }
    public bool IsLastRound { get; set; }
}

public class EvaluationResponse
{
    public double Score { get; set; }
    public double MaxScore { get; set; } = 10.0;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public Dictionary<string, double> RubricBreakdown { get; set; } = new();
    public string? DifficultyAdjustment { get; set; }
}

public class FinalReportResponse
{
    public Guid SessionId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string CompanyMode { get; set; } = string.Empty;
    public double OverallScore { get; set; }
    public int TotalRounds { get; set; }
    public List<RoundSummary> Rounds { get; set; } = new();
    public List<string> OverallStrengths { get; set; } = new();
    public List<string> OverallWeaknesses { get; set; } = new();
    public List<string> ImprovementSuggestions { get; set; } = new();
    public Dictionary<string, double> AggregatedRubric { get; set; } = new();
    public string? ArchitectureDiagram { get; set; }
}

public class RoundSummary
{
    public int RoundNumber { get; set; }
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public double Score { get; set; }
    public string? Difficulty { get; set; }
    public List<string>? Strengths { get; set; }
    public List<string>? Weaknesses { get; set; }
}

/// <summary>
/// A2A-inspired agent message envelope following JSON-RPC 2.0 pattern.
/// </summary>
public class AgentMessage
{
    public string Jsonrpc { get; set; } = "2.0";
    public string? Method { get; set; }
    public Dictionary<string, object>? Params { get; set; }
    public string? Id { get; set; }
    public object? Result { get; set; }
    public AgentError? Error { get; set; }
}

public class AgentError
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}
