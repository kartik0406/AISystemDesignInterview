# 📐 Architecture Document — AI System Design Interviewer

> **Version:** 2.0 (.NET 10 Migration)
> **Last Updated:** April 2026

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Architecture Diagram](#architecture-diagram)
3. [Component Deep Dive](#component-deep-dive)
4. [Request Flow](#request-flow)
5. [Multi-Agent System (A2A)](#multi-agent-system-a2a)
6. [MCP Tool Layer](#mcp-tool-layer)
7. [RAG Pipeline](#rag-pipeline)
8. [Data Model](#data-model)
9. [Session Management](#session-management)
10. [Configuration System](#configuration-system)
11. [Deployment Architecture](#deployment-architecture)
12. [Technology Decisions](#technology-decisions)

---

## System Overview

The AI System Design Interviewer is a **unified .NET 10 microservice** that simulates adaptive, multi-round system design interviews. It combines:

- **API Gateway** — REST endpoints for the React frontend
- **Agent Orchestration** — Multi-agent routing (A2A pattern)
- **LLM Integration** — Google Gemini API for question generation, evaluation, and hints
- **RAG Pipeline** — Pinecone vector search for grounded, knowledge-backed responses
- **MCP Tools** — Structured tool layer for LLM capabilities

All of this runs in a **single process** — no inter-service HTTP calls, no separate Python service. The entire LLM/RAG logic that previously ran in a separate Python FastAPI service is now ported to C# and executes in-process.

---

## Architecture Diagram

```
                          ┌──────────────────────┐
                          │   React Frontend     │
                          │   (Vercel)           │
                          └──────────┬───────────┘
                                     │ HTTPS
                                     ▼
┌────────────────────────────────────────────────────────────────────┐
│                   .NET 10 ASP.NET Core Service                     │
│                        (Render — Docker)                           │
│                                                                    │
│  ┌──────────────┐                                                  │
│  │ Controllers   │ ◄── InterviewController (REST API)              │
│  │               │     McpToolsController  (MCP endpoints)         │
│  │               │     HealthController    (health + A2A)          │
│  └──────┬───────┘                                                  │
│         │                                                          │
│  ┌──────▼───────┐                                                  │
│  │ Services      │ ◄── InterviewService (orchestration)            │
│  │               │     SessionService   (Redis session memory)     │
│  └──────┬───────┘                                                  │
│         │                                                          │
│  ┌──────▼───────┐                                                  │
│  │ Agents        │ ◄── InterviewAgent   (orchestrator)             │
│  │  (A2A)        │     QuestionAgent    (question generation)      │
│  │               │     EvaluationAgent  (rubric scoring)           │
│  │               │     HintAgent        (progressive hints)        │
│  └──────┬───────┘                                                  │
│         │                                                          │
│  ┌──────▼───────┐                                                  │
│  │ MCP Tools     │ ◄── RagTool      (vector search + fallback)    │
│  │               │     ScoringTool  (5-dimension evaluation)       │
│  │               │     DiagramTool  (Mermaid generation)           │
│  │               │     HintTool     (nudge/direction/partial)      │
│  └──────┬───────┘                                                  │
│         │                                                          │
│  ┌──────▼───────┐                                                  │
│  │ LLM & RAG     │ ◄── GeminiClient      (REST API wrapper)      │
│  │               │     PineconeRetriever  (vector search)          │
│  │               │     Prompts            (template library)       │
│  └──────┬───────┘                                                  │
│         │                                                          │
│  ┌──────▼───────┐                                                  │
│  │ Data Layer    │ ◄── AppDbContext  (EF Core → PostgreSQL)       │
│  │               │     Redis IDatabase (session state)             │
│  └──────────────┘                                                  │
└─────────┬──────────────────┬──────────────┬───────────┬────────────┘
          │                  │              │           │
          ▼                  ▼              ▼           ▼
   ┌──────────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐
   │ Supabase     │  │ Upstash  │  │  Google  │  │ Pinecone │
   │ PostgreSQL   │  │  Redis   │  │  Gemini  │  │ Vector DB│
   └──────────────┘  └──────────┘  └──────────┘  └──────────┘
```

---

## Component Deep Dive

### 1. Controllers (API Layer)

| Controller | File | Responsibility |
|-----------|------|---------------|
| **InterviewController** | `Controllers/InterviewController.cs` | REST endpoints for interview lifecycle (start, answer, session, result, hint, topics) |
| **McpToolsController** | `Controllers/McpToolsController.cs` | MCP tool endpoints (`/tools/*`) for direct tool invocation |
| **HealthController** | `Controllers/HealthController.cs` | Health check + A2A agent card discovery |

### 2. Services (Business Logic)

| Service | File | Responsibility |
|---------|------|---------------|
| **InterviewService** | `Services/InterviewService.cs` | Core orchestration — manages interview state machine, delegates to agents, generates final reports |
| **SessionService** | `Services/SessionService.cs` | Redis-backed conversation memory — stores history, previous questions, and session metadata |

### 3. Agents (A2A Pattern)

| Agent | File | Responsibility |
|-------|------|---------------|
| **InterviewAgent** | `Agents/InterviewAgent.cs` | Orchestrator — routes requests to specialist agents |
| **QuestionAgent** | `Agents/QuestionAgent.cs` | Generates adaptive questions using RAG context + conversation history |
| **EvaluationAgent** | `Agents/EvaluationAgent.cs` | Scores answers on 5 rubric dimensions with company-specific weights |
| **HintAgent** | `Agents/HintAgent.cs` | Provides progressive hints (nudge → direction → partial solution) |

### 4. MCP Tools (LLM Capabilities)

| Tool | File | Responsibility |
|------|------|---------------|
| **RagTool** | `Tools/RagTool.cs` | Pinecone vector search with hardcoded fallback knowledge |
| **ScoringTool** | `Tools/ScoringTool.cs` | 5-dimension rubric scoring via Gemini |
| **DiagramTool** | `Tools/DiagramTool.cs` | Generates validated Mermaid architecture diagrams |
| **HintTool** | `Tools/HintTool.cs` | 3-level progressive hint generation |

### 5. LLM & RAG Layer

| Component | File | Responsibility |
|-----------|------|---------------|
| **GeminiClient** | `Llm/GeminiClient.cs` | Google Gemini REST API wrapper — text generation, JSON generation, and embeddings |
| **Prompts** | `Llm/Prompts.cs` | All prompt templates (question, evaluation, diagram, hints) |
| **PineconeRetriever** | `Rag/PineconeRetriever.cs` | Pinecone REST API client — embeds queries via Gemini, searches vector index |

---

## Request Flow

### Starting an Interview

```
POST /api/v1/interview/start
  { "topic": "Design Netflix", "companyMode": "GOOGLE" }

  1. InterviewController.StartInterview()
  2. InterviewService.StartInterviewAsync()
     a. Create InterviewSession entity → save to PostgreSQL
     b. Store session metadata in Redis
     c. InterviewAgent.RouteToQuestionAgentAsync()
        i.  QuestionAgent fetches RAG context from Pinecone
        ii. Formats prompt with topic, difficulty, company focus areas
        iii. Calls GeminiClient.GenerateJsonAsync()
        iv. Stores question in Redis history
     d. Create InterviewRound entity → save to PostgreSQL
  3. Return InterviewResponse with first question
```

### Submitting an Answer

```
POST /api/v1/interview/answer
  { "sessionId": "...", "answer": "I would use microservices with..." }

  1. InterviewController.SubmitAnswer()
  2. InterviewService.SubmitAnswerAsync()
     a. Load session + rounds from PostgreSQL
     b. InterviewAgent.RouteToEvaluationAgentAsync()
        i.   EvaluationAgent fetches RAG context for reference
        ii.  Gets conversation history from Redis
        iii. Applies company-specific rubric weights
        iv.  Calls ScoringTool → GeminiClient.GenerateJsonAsync()
        v.   Returns 5-dimension rubric scores + feedback
     c. Adjust difficulty based on score
     d. If not last round:
        - InterviewAgent.RouteToQuestionAgentAsync() → next question
        - Create new InterviewRound
     e. If last round:
        - Mark session COMPLETED
  3. Return InterviewResponse with evaluation + next question
```

### Requesting a Hint

```
POST /api/v1/interview/hint
  { "sessionId": "...", "hintLevel": 2 }

  1. InterviewController.RequestHint()
  2. InterviewService.RequestHintAsync()
     a. Load current question from session
     b. InterviewAgent.RouteToHintAgentAsync()
        i.  HintAgent gets conversation history from Redis
        ii. Fetches RAG context
        iii. Selects prompt by level (1=nudge, 2=direction, 3=partial)
        iv. Calls HintTool → GeminiClient.GenerateAsync()
  3. Return hint text
```

---

## Multi-Agent System (A2A)

The system follows the **Agent-to-Agent (A2A) protocol** pattern:

```
                    ┌──────────────────┐
                    │ InterviewAgent   │
                    │  (Orchestrator)  │
                    └────────┬─────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
              ▼              ▼              ▼
    ┌──────────────┐ ┌──────────────┐ ┌──────────┐
    │ QuestionAgent│ │EvaluationAgent│ │HintAgent │
    │              │ │              │ │          │
    │ • RAG lookup │ │ • RAG lookup │ │ • RAG    │
    │ • LLM prompt │ │ • Rubric eval│ │ • 3-level│
    │ • History    │ │ • Weights    │ │ • LLM    │
    └──────────────┘ └──────────────┘ └──────────┘
```

**Agent Discovery:** `GET /.well-known/agent-cards` returns agent capabilities in A2A-compatible format.

**How agents communicate:**
- All agents are singletons registered in .NET DI
- `InterviewAgent` holds references to all specialist agents
- Calls are direct method invocations (no HTTP, no message queues)
- Each agent has an `AgentCard` advertising its capabilities

---

## MCP Tool Layer

The **Model Context Protocol (MCP)** tool layer provides structured capabilities to the LLM:

| Tool | Endpoint | Input | Output |
|------|----------|-------|--------|
| **RAG Query** | `POST /tools/rag/query` | `{ query, topK }` | Ranked text chunks + sources |
| **Generate Question** | `POST /tools/generate-question` | `{ topic, difficulty, companyMode, ... }` | `{ question, topicArea, expectedDepth }` |
| **Score Answer** | `POST /tools/score` | `{ question, answer, companyMode, ... }` | 5-dimension rubric scores + feedback |
| **Generate Diagram** | `POST /tools/diagram` | `{ systemDescription, components }` | Valid Mermaid syntax |
| **Generate Hint** | `POST /tools/hint` | `{ question, hintLevel, ... }` | Progressive hint text |

**Tool Discovery:** `GET /tools/manifest` returns all available tools in MCP format.

---

## RAG Pipeline

```
User Query ("Design Netflix scalability")
        │
        ▼
┌──────────────────┐
│ GeminiClient     │ ── Embed query → 768-dim vector
│ .EmbedAsync()    │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ PineconeRetriever│ ── POST to Pinecone REST API
│ .QueryAsync()    │    with vector + topK=5
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Ranked Results   │ ── Top-K chunks with scores
│ (text, source,   │    Used as context in prompts
│  score)          │
└──────────────────┘

Fallback: If Pinecone returns no results or is unreachable,
          RagTool uses hardcoded knowledge on:
          • Load Balancing    • Caching
          • Database Design   • Microservices
          • Scaling Patterns
```

**Embedding Model:** `gemini-embedding-001` (768 dimensions)
**Vector Index:** Pinecone `sdi-knowledge` (Starter plan)
**Knowledge Source:** Markdown files in `knowledge-base/`

---

## Data Model

### PostgreSQL (Supabase)

```
┌──────────────────────────┐     ┌──────────────────────────┐
│    interview_sessions     │     │     interview_rounds      │
├──────────────────────────┤     ├──────────────────────────┤
│ id            (UUID, PK) │     │ id            (UUID, PK) │
│ topic         (TEXT)     │     │ round_number  (INT)      │
│ company_mode  (TEXT)     │     │ question      (TEXT)     │
│ current_difficulty (TEXT)│     │ user_answer   (TEXT)     │
│ current_round (INT)      │     │ evaluation    (TEXT/JSON)│
│ max_rounds    (INT)      │     │ score         (FLOAT)   │
│ status        (TEXT)     │──┐  │ difficulty    (TEXT)     │
│ started_at    (TIMESTAMP)│  │  │ topic_area    (TEXT)     │
│ completed_at  (TIMESTAMP)│  │  │ answered_at   (TIMESTAMP)│
└──────────────────────────┘  │  │ session_id    (UUID, FK) │
                              └──│                          │
                                 └──────────────────────────┘

┌──────────────────────────┐
│   evaluation_results      │
├──────────────────────────┤
│ id            (UUID, PK) │
│ session_id    (UUID, FK) │
│ overall_score (FLOAT)    │
│ strengths     (TEXT/JSON)│
│ weaknesses    (TEXT/JSON)│
│ suggestions   (TEXT/JSON)│
│ rubric_breakdown (JSON)  │
│ architecture_diagram(TEXT)│
│ generated_at  (TIMESTAMP)│
└──────────────────────────┘
```

**ORM:** Entity Framework Core with Npgsql provider
**Enums stored as strings** for readability in the database

---

## Session Management

Redis is used as a fast-access session store for interview state:

```
Redis Keys:
  sdi:session:{sessionId}:history    → LIST of conversation entries (JSON)
  sdi:session:{sessionId}:questions  → LIST of previously asked questions
  sdi:session:{sessionId}:meta       → HASH of session metadata

TTL: 30 minutes (auto-expiry)
```

**Why Redis + PostgreSQL?**
- **Redis:** Hot data — conversation history for real-time question generation (needs to be fast)
- **PostgreSQL:** Cold data — permanent session records for reports and analytics

---

## Configuration System

ASP.NET Core's layered configuration handles dev/prod separation:

```
appsettings.json                ← Base config (production credentials)
  └── appsettings.Development.json  ← Dev overrides (localhost, empty keys)
        └── Environment Variables       ← Highest priority (Render deployment)
```

| Section | Key Values |
|---------|-----------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Redis:ConnectionString` | Redis connection string (with SSL for Upstash) |
| `App:Gemini:ApiKey` | Google Gemini API key |
| `App:Gemini:Model` | Model name (`gemini-2.5-flash`) |
| `App:Pinecone:ApiKey` | Pinecone API key |
| `App:Pinecone:IndexName` | Vector index name (`sdi-knowledge`) |
| `App:Interview:MaxRounds` | Number of rounds per interview (default: 6) |
| `App:Embedding:Model` | Embedding model (`gemini-embedding-001`) |

---

## Deployment Architecture

```
┌─────────────┐     ┌──────────────────────┐     ┌──────────────┐
│   Vercel     │     │       Render          │     │   Supabase   │
│   (CDN)      │     │    (Docker)           │     │  (Postgres)  │
│              │     │                       │     │              │
│  React App   │────▶│  .NET 10 Service      │────▶│  Sessions    │
│  (static)    │     │  (single container)   │     │  Rounds      │
│              │     │                       │     │  Evaluations │
└─────────────┘     │  Port 8080            │     └──────────────┘
                    │  Health: /api/v1/health│
                    └───────────┬───────────┘
                                │
                    ┌───────────┼───────────┐
                    │           │           │
                    ▼           ▼           ▼
             ┌──────────┐ ┌─────────┐ ┌──────────┐
             │ Upstash  │ │ Gemini  │ │ Pinecone │
             │ Redis    │ │ API     │ │ Vector DB│
             └──────────┘ └─────────┘ └──────────┘
```

**Dockerfile:** Multi-stage build (SDK → publish → ASP.NET runtime)
**Render Blueprint:** `render.yaml` defines the single web service
**Docker Compose:** Local development with Redis container

---

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Language** | C# 13 / .NET 10 | Type-safe, high-performance, single unified service |
| **Framework** | ASP.NET Core | Production-grade web framework with built-in DI, middleware |
| **ORM** | EF Core + Npgsql | First-class PostgreSQL support, code-first migrations |
| **Redis Client** | StackExchange.Redis | Industry-standard .NET Redis client, connection pooling |
| **LLM API** | Raw HttpClient | No official .NET Gemini SDK needed — REST API is simple and reliable |
| **Vector DB** | Pinecone REST API | No SDK dependency — direct HTTP calls keep the project lean |
| **JSON** | System.Text.Json | Built-in, high-performance, camelCase + enum-as-string |
| **Deployment** | Docker on Render | Free tier, auto-deploy from GitHub, health checks |
| **Config** | appsettings.json | Standard .NET config with environment-specific overrides |

### Why Unified Service (vs. Polyglot)?

| Aspect | Before (Java + Python) | After (.NET 10) |
|--------|----------------------|-----------------|
| **Services** | 2 (API Gateway + LLM Service) | 1 unified |
| **Deployment slots** | 2 Render services | 1 Render service |
| **Inter-service latency** | ~100-500ms HTTP calls | 0ms (in-process) |
| **Languages** | Java + Python | C# only |
| **Debugging** | Cross-service tracing | Single process debugging |
| **Cold start** | Both services spin down independently | Single cold start |
