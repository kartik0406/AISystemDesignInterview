namespace SdiApiGateway.Config;

/// <summary>
/// Strongly-typed configuration sections matching appsettings.json.
/// </summary>
public class AppSettings
{
    public GeminiSettings Gemini { get; set; } = new();
    public PineconeSettings Pinecone { get; set; } = new();
    public InterviewSettings Interview { get; set; } = new();
    public EmbeddingSettings Embedding { get; set; } = new();
}

public class GeminiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-flash";
}

public class PineconeSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string IndexName { get; set; } = "sdi-knowledge";
    public string Host { get; set; } = string.Empty;
}

public class InterviewSettings
{
    public int MaxRounds { get; set; } = 6;
    public int SessionTtlMinutes { get; set; } = 30;
    public string DefaultCompanyMode { get; set; } = "GENERAL";
}

public class EmbeddingSettings
{
    public string Model { get; set; } = "gemini-embedding-001";
    public int Dimension { get; set; } = 768;
}
