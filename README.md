# 🧠 AI System Design Interviewer

A production-grade, cloud-deployed **multi-agent GenAI platform** that simulates adaptive system design interviews. Built with **.NET 10 (C#)**, **Google Gemini**, **Pinecone RAG**, and **React** — deployed across **Render**, **Vercel**, **Supabase**, and **Upstash**.

> **Live Demo:** [Frontend (Vercel)](https://systemdesigninterviews-mjsoi6stq-kartik0406s-projects.vercel.app) · [API Gateway (Render)](https://sdi-api-gateway.onrender.com)

---

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                        Frontend (React + Vite)                       │
│                         Hosted on Vercel                             │
└────────────────────────────────┬─────────────────────────────────────┘
                                 │ HTTPS
                                 ▼
┌──────────────────────────────────────────────────────────────────────┐
│           Unified .NET 10 API Gateway + LLM Service                  │
│                       Hosted on Render                               │
│                                                                      │
│  ┌─────────────────────────── Agents ───────────────────────────┐   │
│  │                                                               │   │
│  │  ┌──────────────────┐  ┌─────────────────┐  ┌────────────┐  │   │
│  │  │ Interview Agent   │  │ Question Agent  │  │ Evaluation │  │   │
│  │  │  (Orchestrator)   │──│  (Generates Qs) │  │   Agent    │  │   │
│  │  └──────────────────┘  └─────────────────┘  └────────────┘  │   │
│  │           │                                                   │   │
│  │  ┌────────┴──────────┐                                       │   │
│  │  │   Hint Agent      │                                       │   │
│  │  │ (Progressive Hints)│                                      │   │
│  │  └───────────────────┘                                       │   │
│  └───────────────────────────────────────────────────────────────┘   │
│                                                                      │
│  ┌─────────────────────── MCP Tools ────────────────────────────┐   │
│  │  RAG Tool  │  Scoring Tool  │  Diagram Tool  │  Hint Tool   │   │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                      │
│  ┌─────────────────────── Services ─────────────────────────────┐   │
│  │  InterviewService │ SessionService │ GeminiClient │ Pinecone │   │
│  └──────────────────────────────────────────────────────────────┘   │
│           │                    │              │           │          │
└───────────┼────────────────────┼──────────────┼───────────┼──────────┘
            │                    │              │           │
            ▼                    ▼              ▼           ▼
     ┌───────────────┐   ┌──────────┐   ┌──────────┐  ┌──────────┐
     │   Supabase     │   │ Upstash  │   │  Google  │  │ Pinecone │
     │  PostgreSQL    │   │  Redis   │   │  Gemini  │  │ Vector DB│
     └───────────────┘   └──────────┘   └──────────┘  └──────────┘
```

---

## ⚡ Tech Stack

| Layer | Technology | Hosting |
|-------|-----------|---------|
| **Backend** | .NET 10, ASP.NET Core, C# 13 | **Render** (Docker, Free) |
| **Frontend** | React 19, Vite 5, Vanilla CSS | **Vercel** (Free) |
| **Database** | PostgreSQL 15 (EF Core) | **Supabase** (Free) |
| **Session Memory** | Redis 7 (StackExchange.Redis) | **Upstash** (Free) |
| **Vector DB** | Pinecone (Gemini Embeddings, 768d) | **Pinecone** (Starter, Free) |
| **LLM Provider** | Google Gemini 2.5 Flash | **Google AI Studio** (Free) |

---

## 🎯 Key Features

- **Unified .NET 10 Microservice** — Single deployable service (API Gateway + LLM logic), zero inter-service latency
- **Adaptive Difficulty** — Questions adjust based on answer quality (score ≥ 8 → harder, ≤ 4 → easier)
- **Multi-Agent Architecture (A2A)** — Specialized agents for question generation, evaluation, and hints
- **MCP Tool Layer** — In-process LLM tools for RAG retrieval, rubric scoring, Mermaid diagrams, and progressive hints
- **RAG Knowledge Base** — Pinecone-powered retrieval with Gemini Embeddings for grounded feedback
- **Structured Rubric Scoring** — 5-dimension evaluation: Scalability, Database Design, API Design, Trade-offs, Clarity
- **Company Modes** — Tailored interviews for Google (scalability focus), Amazon (trade-offs), and General
- **Progressive Hints** — 3-level hint system: Nudge → Direction → Partial Solution
- **Architecture Diagrams** — Auto-generated Mermaid diagrams for system visualization
- **Full Cloud Deployment** — Zero infrastructure, 100% free-tier hosted

---

## 🚀 Quick Start

### Option A: Cloud (Production)

The app is fully deployed and accessible via the live demo links above. No setup required.

### Option B: Local Development

#### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Node.js 20+
- Docker (for Redis, or use Upstash)
- Google Gemini API key

#### 1. Clone and configure
```bash
git clone https://github.com/kartik0406/SystemDesignInterview.git
cd SystemDesignInterview
```

#### 2. Configure API keys

Edit `api-gateway-dotnet/appsettings.Development.json` and add your API keys:
```json
{
  "App": {
    "Gemini": { "ApiKey": "your-gemini-key" },
    "Pinecone": { "ApiKey": "your-pinecone-key" }
  }
}
```

#### 3. Start with Docker Compose
```bash
docker-compose up -d
```

#### 4. Or run services individually

**Redis (local):**
```bash
docker run -d -p 6379:6379 redis:7-alpine
```

**API Gateway (.NET 10):**
```bash
cd api-gateway-dotnet
dotnet run
```

**Frontend (React):**
```bash
cd frontend
npm install && npm run dev
```

#### 5. Open in browser
- Frontend: `http://localhost:5173`
- Swagger UI: `http://localhost:8080/swagger`
- Health check: `http://localhost:8080/api/v1/health`

---

## 🔌 API Endpoints

### Interview Flow
| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/v1/interview/start` | Start a new interview session |
| `POST` | `/api/v1/interview/answer` | Submit an answer for evaluation |
| `GET` | `/api/v1/interview/session/{id}` | Get current session state |
| `GET` | `/api/v1/interview/result/{id}` | Get final interview report |
| `POST` | `/api/v1/interview/hint` | Request a progressive hint |
| `GET` | `/api/v1/interview/topics` | List available topics |

### Agent Discovery & Tools (MCP)
| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/.well-known/agent-cards` | A2A agent discovery |
| `GET` | `/tools/manifest` | MCP tool manifest |
| `POST` | `/tools/rag/query` | RAG knowledge retrieval |
| `POST` | `/tools/generate-question` | Generate interview question |
| `POST` | `/tools/score` | Evaluate candidate answer |
| `POST` | `/tools/diagram` | Generate Mermaid diagram |
| `POST` | `/tools/hint` | Generate progressive hint |

---

## 📁 Project Structure

```
SystemDesignInterviewNET/
├── api-gateway-dotnet/            # .NET 10 ASP.NET Core — Unified API + LLM
│   ├── Dockerfile
│   ├── SdiApiGateway.csproj       # .NET 10 project (net10.0)
│   ├── Program.cs                 # App entry: DI, middleware, DB, Redis, CORS
│   ├── appsettings.json           # Production config (Supabase, Upstash, keys)
│   ├── appsettings.Development.json # Local dev config (localhost, empty keys)
│   │
│   ├── Controllers/
│   │   ├── InterviewController.cs # Interview lifecycle REST endpoints
│   │   ├── HealthController.cs    # Health + A2A agent discovery
│   │   └── McpToolsController.cs  # MCP tool endpoints (/tools/*)
│   │
│   ├── Models/
│   │   ├── Entities/              # EF Core entities (Session, Round, Evaluation)
│   │   ├── DTOs/                  # Request/Response types
│   │   ├── Enums/                 # CompanyMode, DifficultyLevel, SessionStatus
│   │   └── McpSchemas/            # MCP tool request/response models
│   │
│   ├── Services/
│   │   ├── InterviewService.cs    # Core orchestration logic
│   │   └── SessionService.cs      # Redis-backed session memory
│   │
│   ├── Agents/
│   │   ├── InterviewAgent.cs      # Orchestrator — routes to specialists
│   │   ├── QuestionAgent.cs       # Generates adaptive questions (RAG + LLM)
│   │   ├── EvaluationAgent.cs     # Scores answers with rubric
│   │   ├── HintAgent.cs           # Progressive hints
│   │   └── AgentCard.cs           # A2A-compatible agent cards
│   │
│   ├── Llm/
│   │   ├── GeminiClient.cs        # Gemini REST API (text, JSON, embeddings)
│   │   └── Prompts.cs             # All prompt templates
│   │
│   ├── Rag/
│   │   └── PineconeRetriever.cs   # Pinecone vector search via REST
│   │
│   ├── Tools/
│   │   ├── RagTool.cs             # RAG retrieval + fallback knowledge
│   │   ├── ScoringTool.cs         # 5-dimension rubric evaluation
│   │   ├── DiagramTool.cs         # Mermaid diagram generation
│   │   └── HintTool.cs            # Progressive hint generation
│   │
│   ├── Data/
│   │   └── AppDbContext.cs        # EF Core DbContext (PostgreSQL)
│   │
│   ├── Config/
│   │   └── AppSettings.cs         # Strongly-typed config sections
│   │
│   └── Middleware/
│       └── GlobalExceptionHandler.cs
│
├── frontend/                      # React + Vite — Premium UI
│   ├── Dockerfile
│   ├── vercel.json                # Vercel SPA routing config
│   └── src/
│       ├── App.jsx                # Router (Landing → Interview → Results)
│       ├── api.js                 # API client (axios)
│       └── pages/
│           ├── LandingPage.jsx    # Topic selection + company mode
│           ├── InterviewPage.jsx  # Chat interface + hint system
│           └── ResultsPage.jsx    # Scores, rubric breakdown, diagram
│
├── knowledge-base/                # System design knowledge for RAG
│   ├── patterns/                  # Caching, Sharding, CAP, Load Balancing
│   └── architectures/             # URL Shortener, Netflix, Uber designs
│
├── render.yaml                    # Render Blueprint (single .NET service)
├── docker-compose.yml             # Local development orchestration
└── .env.example                   # Environment variable template
```

---

## ☁️ Cloud Deployment

This project is deployed using **free tiers** across multiple cloud providers:

| Service | Provider | Config |
|---------|----------|--------|
| API Gateway + LLM | [Render](https://render.com) | `render.yaml`, Docker |
| Frontend | [Vercel](https://vercel.com) | `frontend/vercel.json` |
| Database | [Supabase](https://supabase.com) | `appsettings.json` |
| Redis | [Upstash](https://upstash.com) | `appsettings.json` |
| Vector DB | [Pinecone](https://pinecone.io) | `appsettings.json` |

### Configuration

All config is managed via `appsettings.json` (production) and `appsettings.Development.json` (local dev). Environment variables on Render override these values automatically.

---

## 🧪 Interview Flow

```
User selects topic (e.g., "Design Netflix") + company mode
        │
        ▼
InterviewAgent creates session → stores in Supabase (PostgreSQL)
        │
        ▼
QuestionAgent fetches RAG context from Pinecone
        │
        ▼
Gemini generates adaptive question based on:
  • Topic + difficulty level
  • Previous Q&A history (Redis)
  • RAG knowledge context
        │
        ▼
User submits answer
        │
        ▼
EvaluationAgent scores on 5 rubric dimensions:
  • Scalability, Database Design, API Design, Trade-offs, Clarity
        │
        ▼
Difficulty adjusts: score ≥ 8 → harder, ≤ 4 → easier
        │
        ▼
Repeat for 6 rounds → Final Report with:
  • Overall score
  • Rubric breakdown (radial chart)
  • Strengths / Weaknesses / Suggestions
  • Architecture diagram (Mermaid)
```

---

## 💡 Resume Bullet Points

- Architected a **multi-agent GenAI platform** using A2A patterns in **.NET 10 / C#** to simulate adaptive system design interviews with Google Gemini
- Implemented an **MCP-based tool layer** enabling LLMs to perform RAG retrieval, rubric scoring, and Mermaid diagram generation — all in-process
- Built a **RAG pipeline** using Pinecone + Gemini Embeddings (768d, Matryoshka) for contextual interview evaluation grounding
- Developed a **unified ASP.NET Core microservice** with EF Core/PostgreSQL persistence, StackExchange.Redis session memory, and adaptive difficulty scaling
- Deployed to production using **Render (IaC via render.yaml)**, **Vercel**, **Supabase**, **Upstash**, and **Pinecone** — fully on free tiers
- Created a **premium React UI** with animated scoring, radial charts, glassmorphism design, and real-time interview flow

---

## 📄 License

MIT
