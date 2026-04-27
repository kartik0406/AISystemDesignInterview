using Microsoft.EntityFrameworkCore;
using SdiApiGateway.Agents;
using SdiApiGateway.Config;
using SdiApiGateway.Data;
using SdiApiGateway.Llm;
using SdiApiGateway.Middleware;
using SdiApiGateway.Rag;
using SdiApiGateway.Services;
using SdiApiGateway.Tools;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ─── Configuration ───────────────────────────────────────────
// All credentials live in appsettings.json (production) and
// appsettings.Development.json (local dev).
// Env vars still override appsettings when set (e.g. on Render).

// Bind strongly-typed config
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));

// ─── Database (EF Core + PostgreSQL) ─────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Redis ───────────────────────────────────────────────────
var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = ConfigurationOptions.Parse(redisConnectionString);
    config.AbortOnConnectFail = false;
    config.ConnectTimeout = 5000;
    return ConnectionMultiplexer.Connect(config);
});

// ─── HttpClient ──────────────────────────────────────────────
builder.Services.AddHttpClient<GeminiClient>();
builder.Services.AddHttpClient<PineconeRetriever>();

// ─── Services ────────────────────────────────────────────────
builder.Services.AddSingleton<SessionService>();

// LLM & RAG layer
builder.Services.AddSingleton<GeminiClient>();
builder.Services.AddSingleton<PineconeRetriever>();

// MCP Tools
builder.Services.AddSingleton<RagTool>();
builder.Services.AddSingleton<ScoringTool>();
builder.Services.AddSingleton<DiagramTool>();
builder.Services.AddSingleton<HintTool>();

// Agents
builder.Services.AddSingleton<QuestionAgent>();
builder.Services.AddSingleton<EvaluationAgent>();
builder.Services.AddSingleton<HintAgent>();
builder.Services.AddSingleton<InterviewAgent>();

// Interview service (scoped — uses DbContext)
builder.Services.AddScoped<InterviewService>();

// Exception handler middleware
builder.Services.AddTransient<GlobalExceptionHandler>();

// ─── Controllers & JSON ──────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ─── Swagger / OpenAPI ───────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─── CORS ────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ─── Build App ───────────────────────────────────────────────
var app = builder.Build();

// ─── Middleware Pipeline ─────────────────────────────────────
app.UseMiddleware<GlobalExceptionHandler>();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// ─── Auto-migrate database ──────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Database migration failed (may need manual setup). Trying EnsureCreated...");
        try { db.Database.EnsureCreated(); }
        catch { /* DB might not be reachable in dev without config */ }
    }
}

// ─── Configure port ──────────────────────────────────────────
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
