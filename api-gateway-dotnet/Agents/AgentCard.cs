namespace SdiApiGateway.Agents;

/// <summary>
/// A2A-compatible agent card for service discovery.
/// Each agent advertises its capabilities.
/// </summary>
public class AgentCard
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public List<string> Capabilities { get; set; } = new();
    public List<string> SupportedMethods { get; set; } = new();

    public static AgentCard QuestionAgent() => new()
    {
        Name = "QuestionAgent",
        Description = "Generates adaptive system design interview questions",
        Version = "1.0.0",
        Capabilities = new() { "question_generation", "difficulty_adjustment", "topic_selection" },
        SupportedMethods = new() { "generate_question" }
    };

    public static AgentCard EvaluationAgent() => new()
    {
        Name = "EvaluationAgent",
        Description = "Evaluates candidate answers with structured rubric scoring",
        Version = "1.0.0",
        Capabilities = new() { "answer_evaluation", "rubric_scoring", "feedback_generation" },
        SupportedMethods = new() { "evaluate_answer" }
    };

    public static AgentCard HintAgent() => new()
    {
        Name = "HintAgent",
        Description = "Provides progressive hints without revealing full solutions",
        Version = "1.0.0",
        Capabilities = new() { "hint_generation", "progressive_disclosure" },
        SupportedMethods = new() { "provide_hint" }
    };
}
