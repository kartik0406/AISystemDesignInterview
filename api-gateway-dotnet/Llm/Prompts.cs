namespace SdiApiGateway.Llm;

/// <summary>
/// All prompt templates for LLM interactions.
/// Centralized for easy modification and A/B testing.
/// Ported from Python llm/prompts.py.
/// </summary>
public static class Prompts
{
    // ─── Question Generation ─────────────────────────────────────

    public const string QuestionGenerator = """
        You are a senior system design interviewer at {0}.
        You are conducting a system design interview on the topic: "{1}".

        Current difficulty level: {2}/10

        Previous questions asked (DO NOT repeat these):
        {3}

        Recent conversation context:
        {4}

        Relevant system design knowledge:
        {5}

        Focus areas for this company: {6}

        Generate a single, focused follow-up question that:
        1. Does NOT repeat any previous questions
        2. Probes deeper based on the candidate's previous answers
        3. Matches the difficulty level ({2}/10)
        4. Focuses on one of the specified focus areas
        5. Is specific enough to evaluate the candidate's understanding
        6. Encourages the candidate to think about trade-offs

        Return ONLY a JSON object (no markdown, no code fences):
        {{"question": "Your question here", "topic_area": "e.g., scalability/caching/database", "expected_depth": "overview/detailed/expert"}}
        """;

    // ─── Evaluation ───────────────────────────────────────────────

    public const string Evaluation = """
        You are a senior system design interviewer at {0}.
        Evaluate this candidate's answer thoroughly and fairly.

        Question: {1}
        Candidate's Answer: {2}

        Reference context from system design knowledge base:
        {3}

        Conversation history:
        {4}

        Evaluate on these dimensions (score each 0-10):
        1. **Scalability** - Understanding of horizontal/vertical scaling, load balancing, partitioning
        2. **Database Design** - Schema choices, indexing, replication, consistency models
        3. **API Design** - Clean interfaces, proper resource modeling, error handling
        4. **Trade-offs** - Ability to identify and articulate design trade-offs
        5. **Clarity** - Communication quality, structure of explanation, use of examples

        Rubric weights for {0}: {5}

        Return ONLY a JSON object (no markdown, no code fences):
        {{
          "score": <weighted overall score 0-10>,
          "maxScore": 10,
          "strengths": ["strength 1", "strength 2", "strength 3"],
          "weaknesses": ["weakness 1", "weakness 2"],
          "suggestions": ["suggestion 1", "suggestion 2"],
          "rubricBreakdown": {{
            "scalability": <0-10>,
            "database_design": <0-10>,
            "api_design": <0-10>,
            "tradeoffs": <0-10>,
            "clarity": <0-10>
          }},
          "difficultyAdjustment": "<increase|maintain|decrease>"
        }}
        """;

    // ─── Diagram Generation ──────────────────────────────────────

    public const string Diagram = """
        Generate a Mermaid architecture diagram for: {0}

        The diagram should show the key components and their interactions.
        Use a clear, professional layout with meaningful labels.

        Additional components to include: {1}

        Return ONLY valid Mermaid syntax (no markdown code fences, no explanation).
        Use 'graph TB' or 'graph LR' format.
        Example format:
        graph TB
            Client[Client/Browser] --> LB[Load Balancer]
            LB --> API[API Server]
            API --> Cache[(Redis Cache)]
            API --> DB[(Database)]
        """;

    // ─── Hint Generation ─────────────────────────────────────────

    public static readonly Dictionary<int, string> Hints = new()
    {
        [1] = """
            The candidate is working on this system design question:
            "{0}"

            Previous conversation:
            {1}

            Provide a NUDGE — a very brief, high-level hint that points them in the right direction without revealing any specifics.
            The nudge should be 1-2 sentences maximum.
            Do NOT reveal the answer or specific technologies.

            Return ONLY the hint text, nothing else.
            """,

        [2] = """
            The candidate is working on this system design question:
            "{0}"

            Previous conversation:
            {1}

            Relevant knowledge:
            {2}

            Provide a DIRECTION — a hint that identifies the specific area or concept they should focus on.
            Mention the category of solution (e.g., "Think about caching strategies") but don't give the full solution.
            The direction should be 2-3 sentences.

            Return ONLY the hint text, nothing else.
            """,

        [3] = """
            The candidate is working on this system design question:
            "{0}"

            Previous conversation:
            {1}

            Relevant knowledge:
            {2}

            Provide a PARTIAL SOLUTION — give them a concrete starting point with specific technologies or patterns to consider.
            Include one specific example but leave room for them to fill in the details.
            The partial solution should be 3-4 sentences.

            Return ONLY the hint text, nothing else.
            """
    };
}
