using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SdiApiGateway.Agents;
using SdiApiGateway.Config;
using SdiApiGateway.Data;
using SdiApiGateway.Middleware;
using SdiApiGateway.Models.DTOs;
using SdiApiGateway.Models.Entities;
using SdiApiGateway.Models.Enums;
using SdiApiGateway.Tools;

namespace SdiApiGateway.Services;

/// <summary>
/// Core interview orchestration service.
/// Manages the interview lifecycle: start → answer → evaluate → next question → report.
/// Ported from Java InterviewService.
/// </summary>
public class InterviewService
{
    private readonly AppDbContext _db;
    private readonly InterviewAgent _interviewAgent;
    private readonly DiagramTool _diagramTool;
    private readonly SessionService _sessionService;
    private readonly AppSettings _appSettings;
    private readonly ILogger<InterviewService> _logger;

    public static readonly List<Dictionary<string, string>> AvailableTopics = new()
    {
        new() { ["id"] = "url-shortener", ["name"] = "Design URL Shortener", ["description"] = "Design a URL shortening service like bit.ly" },
        new() { ["id"] = "twitter", ["name"] = "Design Twitter", ["description"] = "Design a social media feed system" },
        new() { ["id"] = "netflix", ["name"] = "Design Netflix", ["description"] = "Design a video streaming platform" },
        new() { ["id"] = "uber", ["name"] = "Design Uber", ["description"] = "Design a ride-sharing service" },
        new() { ["id"] = "whatsapp", ["name"] = "Design WhatsApp", ["description"] = "Design a real-time messaging system" },
        new() { ["id"] = "instagram", ["name"] = "Design Instagram", ["description"] = "Design a photo-sharing social network" },
        new() { ["id"] = "rate-limiter", ["name"] = "Design Rate Limiter", ["description"] = "Design a distributed rate limiting system" },
        new() { ["id"] = "notification", ["name"] = "Design Notification System", ["description"] = "Design a scalable notification service" },
        new() { ["id"] = "search-engine", ["name"] = "Design Search Engine", ["description"] = "Design a web search engine like Google" },
        new() { ["id"] = "payment", ["name"] = "Design Payment System", ["description"] = "Design a payment processing platform" }
    };

    public InterviewService(AppDbContext db, InterviewAgent interviewAgent,
        DiagramTool diagramTool, SessionService sessionService,
        IOptions<AppSettings> appSettings, ILogger<InterviewService> logger)
    {
        _db = db;
        _interviewAgent = interviewAgent;
        _diagramTool = diagramTool;
        _sessionService = sessionService;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    // ─── Start Interview ─────────────────────────────────────

    public async Task<InterviewResponse> StartInterviewAsync(StartInterviewRequest request)
    {
        _logger.LogInformation("Starting interview: topic={Topic}, company={Company}", request.Topic, request.CompanyMode);

        var session = new InterviewSession
        {
            Topic = request.Topic,
            CompanyMode = request.CompanyMode,
            CurrentDifficulty = DifficultyLevel.MEDIUM,
            CurrentRound = 1,
            MaxRounds = _appSettings.Interview.MaxRounds,
            Status = SessionStatus.IN_PROGRESS
        };

        _db.InterviewSessions.Add(session);
        await _db.SaveChangesAsync();

        // Store session metadata in Redis
        _sessionService.SetSessionMeta(session.Id, "topic", request.Topic);
        _sessionService.SetSessionMeta(session.Id, "companyMode", request.CompanyMode.ToString());

        // Generate first question via QuestionAgent
        var questionResult = await _interviewAgent.RouteToQuestionAgentAsync(
            session.Id, request.Topic, request.CompanyMode,
            DifficultyLevel.MEDIUM, new List<Dictionary<string, string>>());

        var question = questionResult.GetValueOrDefault("question", $"Let's start with the high-level design. How would you approach {request.Topic}?")?.ToString() ?? "";
        var topicArea = questionResult.GetValueOrDefault("topic_area", "high-level design")?.ToString() ?? "";

        // Create first round
        var round = new InterviewRound
        {
            RoundNumber = 1,
            Question = question,
            Difficulty = DifficultyLevel.MEDIUM,
            TopicArea = topicArea
        };

        session.AddRound(round);
        await _db.SaveChangesAsync();

        return new InterviewResponse
        {
            SessionId = session.Id,
            Topic = session.Topic,
            CompanyMode = session.CompanyMode,
            CurrentRound = 1,
            MaxRounds = session.MaxRounds,
            Difficulty = DifficultyLevel.MEDIUM,
            Status = SessionStatus.IN_PROGRESS,
            Question = question,
            TopicArea = topicArea,
            IsLastRound = false
        };
    }

    // ─── Submit Answer ───────────────────────────────────────

    public async Task<InterviewResponse> SubmitAnswerAsync(SubmitAnswerRequest request)
    {
        var session = await _db.InterviewSessions
            .Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId)
            ?? throw new InterviewException($"Session not found: {request.SessionId}");

        if (session.Status != SessionStatus.IN_PROGRESS)
            throw new InterviewException("Interview session is not in progress");

        var currentRound = session.Rounds.FirstOrDefault(r => r.RoundNumber == session.CurrentRound)
            ?? throw new InterviewException("Current round not found");

        // Store user answer
        currentRound.UserAnswer = request.Answer;
        currentRound.AnsweredAt = DateTime.UtcNow;

        // Evaluate via EvaluationAgent
        var evaluation = await _interviewAgent.RouteToEvaluationAgentAsync(
            session.Id, currentRound.Question, request.Answer, session.CompanyMode);

        currentRound.Score = evaluation.Score;
        try
        {
            currentRound.Evaluation = JsonSerializer.Serialize(evaluation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize evaluation");
        }

        // Adjust difficulty based on score
        var newDifficulty = session.CurrentDifficulty.Adjust(evaluation.Score);
        session.CurrentDifficulty = newDifficulty;

        var isLastRound = session.CurrentRound >= session.MaxRounds;

        if (isLastRound)
        {
            session.Status = SessionStatus.COMPLETED;
            session.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new InterviewResponse
            {
                SessionId = session.Id,
                Topic = session.Topic,
                CompanyMode = session.CompanyMode,
                CurrentRound = session.CurrentRound,
                MaxRounds = session.MaxRounds,
                Difficulty = newDifficulty,
                Status = SessionStatus.COMPLETED,
                Evaluation = evaluation,
                IsLastRound = true
            };
        }

        // Generate next question
        session.CurrentRound++;
        var recentHistory = _sessionService.GetRecentHistory(session.Id, 3);

        var nextQuestionResult = await _interviewAgent.RouteToQuestionAgentAsync(
            session.Id, session.Topic, session.CompanyMode, newDifficulty, recentHistory);

        var nextQuestion = nextQuestionResult.GetValueOrDefault("question", "Can you elaborate on your design choices?")?.ToString() ?? "";
        var nextTopicArea = nextQuestionResult.GetValueOrDefault("topic_area", "deep-dive")?.ToString() ?? "";

        var nextRound = new InterviewRound
        {
            RoundNumber = session.CurrentRound,
            Question = nextQuestion,
            Difficulty = newDifficulty,
            TopicArea = nextTopicArea
        };

        session.AddRound(nextRound);
        await _db.SaveChangesAsync();

        return new InterviewResponse
        {
            SessionId = session.Id,
            Topic = session.Topic,
            CompanyMode = session.CompanyMode,
            CurrentRound = session.CurrentRound,
            MaxRounds = session.MaxRounds,
            Difficulty = newDifficulty,
            Status = SessionStatus.IN_PROGRESS,
            Question = nextQuestion,
            TopicArea = nextTopicArea,
            Evaluation = evaluation,
            IsLastRound = session.CurrentRound >= session.MaxRounds
        };
    }

    // ─── Get Session State ───────────────────────────────────

    public async Task<InterviewResponse> GetSessionAsync(Guid sessionId)
    {
        var session = await _db.InterviewSessions
            .Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new InterviewException($"Session not found: {sessionId}");

        var latestRound = session.Rounds.MaxBy(r => r.RoundNumber);

        return new InterviewResponse
        {
            SessionId = session.Id,
            Topic = session.Topic,
            CompanyMode = session.CompanyMode,
            CurrentRound = session.CurrentRound,
            MaxRounds = session.MaxRounds,
            Difficulty = session.CurrentDifficulty,
            Status = session.Status,
            Question = latestRound?.Question,
            TopicArea = latestRound?.TopicArea,
            IsLastRound = session.CurrentRound >= session.MaxRounds
        };
    }

    // ─── Generate Final Report ───────────────────────────────

    public async Task<FinalReportResponse> GenerateReportAsync(Guid sessionId)
    {
        var session = await _db.InterviewSessions
            .Include(s => s.Rounds)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new InterviewException($"Session not found: {sessionId}");

        var roundSummaries = new List<RoundSummary>();
        var allStrengths = new List<string>();
        var allWeaknesses = new List<string>();
        var rubricAccumulator = new Dictionary<string, List<double>>();

        foreach (var round in session.Rounds)
        {
            var summary = new RoundSummary
            {
                RoundNumber = round.RoundNumber,
                Question = round.Question,
                Answer = round.UserAnswer,
                Score = round.Score ?? 0,
                Difficulty = round.Difficulty?.ToString() ?? "MEDIUM"
            };

            if (!string.IsNullOrEmpty(round.Evaluation))
            {
                try
                {
                    var eval = JsonSerializer.Deserialize<EvaluationResponse>(round.Evaluation,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (eval != null)
                    {
                        summary.Strengths = eval.Strengths;
                        summary.Weaknesses = eval.Weaknesses;
                        allStrengths.AddRange(eval.Strengths);
                        allWeaknesses.AddRange(eval.Weaknesses);

                        foreach (var (key, value) in eval.RubricBreakdown)
                        {
                            if (!rubricAccumulator.ContainsKey(key))
                                rubricAccumulator[key] = new List<double>();
                            rubricAccumulator[key].Add(value);
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning("Failed to parse evaluation for round {RoundNumber}: {Error}",
                        round.RoundNumber, ex.Message);
                }
            }

            roundSummaries.Add(summary);
        }

        // Calculate overall score
        var scoredRounds = session.Rounds.Where(r => r.Score.HasValue).ToList();
        var overallScore = scoredRounds.Count > 0 ? scoredRounds.Average(r => r.Score!.Value) : 0.0;

        // Aggregate rubric scores
        var aggregatedRubric = rubricAccumulator.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Average());

        var uniqueStrengths = allStrengths.Distinct().Take(5).ToList();
        var uniqueWeaknesses = allWeaknesses.Distinct().Take(5).ToList();
        var suggestions = GenerateSuggestions(uniqueWeaknesses);

        // Generate architecture diagram
        var diagramResult = await _diagramTool.GenerateAsync(new() { SystemDescription = session.Topic });

        return new FinalReportResponse
        {
            SessionId = session.Id,
            Topic = session.Topic,
            CompanyMode = session.CompanyMode.ToString(),
            OverallScore = Math.Round(overallScore, 1),
            TotalRounds = roundSummaries.Count,
            Rounds = roundSummaries,
            OverallStrengths = uniqueStrengths,
            OverallWeaknesses = uniqueWeaknesses,
            ImprovementSuggestions = suggestions,
            AggregatedRubric = aggregatedRubric,
            ArchitectureDiagram = diagramResult.Diagram
        };
    }

    // ─── Request Hint ────────────────────────────────────────

    public async Task<string> RequestHintAsync(Models.DTOs.HintRequest request)
    {
        var session = await _db.InterviewSessions
            .Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId)
            ?? throw new InterviewException($"Session not found: {request.SessionId}");

        if (session.Status != SessionStatus.IN_PROGRESS)
            throw new InterviewException("Interview is not in progress");

        var currentRound = session.Rounds.FirstOrDefault(r => r.RoundNumber == session.CurrentRound)
            ?? throw new InterviewException("Current round not found");

        return await _interviewAgent.RouteToHintAgentAsync(
            session.Id, currentRound.Question, request.HintLevel);
    }

    // ─── Get Available Topics ────────────────────────────────

    public List<Dictionary<string, string>> GetTopics() => AvailableTopics;

    // ─── Helpers ─────────────────────────────────────────────

    private static List<string> GenerateSuggestions(List<string> weaknesses)
    {
        var suggestions = new List<string>();
        foreach (var weakness in weaknesses)
        {
            var lower = weakness.ToLower();
            if (lower.Contains("shard") || lower.Contains("partition"))
                suggestions.Add("Study database sharding and partitioning strategies (consistent hashing, range-based, hash-based)");
            if (lower.Contains("consisten") || lower.Contains("cap"))
                suggestions.Add("Deep dive into CAP theorem and consistency models (eventual, strong, causal)");
            if (lower.Contains("cache") || lower.Contains("caching"))
                suggestions.Add("Explore caching patterns: write-through, write-behind, cache-aside, and invalidation strategies");
            if (lower.Contains("api") || lower.Contains("interface"))
                suggestions.Add("Practice designing clean REST APIs with proper resource modeling and versioning");
            if (lower.Contains("scale") || lower.Contains("scalab"))
                suggestions.Add("Study horizontal vs vertical scaling, load balancing, and auto-scaling patterns");
            if (lower.Contains("trade") || lower.Contains("tradeoff"))
                suggestions.Add("Practice articulating trade-offs: latency vs throughput, consistency vs availability, cost vs performance");
        }

        if (suggestions.Count == 0)
        {
            suggestions.Add("Review system design fundamentals: scalability, reliability, and maintainability");
            suggestions.Add("Practice drawing architecture diagrams and explaining component interactions");
        }

        return suggestions.Distinct().ToList();
    }
}
