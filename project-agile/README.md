# Project AGILE — Evaluation Module

**AI Governance & LLM Integrity Evaluation**

A **C# / .NET 8** CLI and **Blazor Server** web application that benchmarks LLMs in the bank's RAG workflows — measuring bias, faithfulness, accuracy, latency, and cost. Built for the AICoE and designed to produce audit-ready output.

---

## Free APIs Supported (Zero Cost to Start)

| Provider | Models Available | Free Tier | Where to Get Key |
| :--- | :--- | :--- | :--- |
| **GitHub Models** | GPT-4o, GPT-4o-mini, Claude 3.5 Sonnet, Llama 3.1 70B, Mistral Large, Phi-4 | ✅ Included with GitHub Copilot Pro | [github.com/marketplace/models](https://github.com/marketplace/models) |
| **Google Gemini** | Gemini 1.5 Flash (1,500/day), Gemini 1.5 Pro (50/day) | ✅ Free tier | [aistudio.google.com/apikey](https://aistudio.google.com/apikey) |
| **NVIDIA NIM** | Llama 3.1 70B, Mistral 7B, Mixtral 8x22B, Phi-3, and more | ✅ 1,000 free credits | [build.nvidia.com](https://build.nvidia.com) |

> All three providers use the **OpenAI-compatible API format**. In C#, you use one `OpenAIClient` for all of them — just change the base URL and API key.

---

## Tech Stack

| Layer | Technology |
| :--- | :--- |
| **CLI Tool** | C# / .NET 8 console app |
| **Web Application** | C# / .NET 8 / Blazor Server |
| **Shared Logic** | `Agile.Core` class library (shared between CLI and web app) |
| **LLM Client** | `OpenAI` .NET SDK (works for all three free providers) |
| **CLI UX** | Spectre.Console.Cli |
| **UI Components** | MudBlazor |
| **Config Parsing** | YamlDotNet |
| **Report Templating** | Scriban (HTML reports) |

---

## Solution Structure

```
Agile.sln
├── Agile.Core/              ← Shared logic: metrics, model clients, runners, parsers
│   ├── Models/              ← Data models (TestCase, EvaluationResult, RunReport)
│   ├── Clients/             ← One ModelClient interface, three implementations
│   │   ├── IModelClient.cs
│   │   ├── GitHubModelsClient.cs
│   │   ├── GeminiClient.cs
│   │   └── NvidiaClient.cs
│   ├── Metrics/             ← Faithfulness, Bias, Relevancy, Cost calculators
│   ├── Runner/              ← Evaluation orchestration
│   └── Reports/             ← JSON + HTML report generation
│
├── Agile.Cli/               ← CLI entry point
│   ├── Program.cs
│   └── Commands/
│       ├── RunCommand.cs
│       ├── ReportCommand.cs
│       ├── ValidateCommand.cs
│       └── ModelsCommand.cs
│
├── Agile.Web/               ← Blazor Server web application
│   ├── Pages/
│   │   ├── Dashboard.razor
│   │   ├── TestRunner.razor
│   │   ├── Results.razor
│   │   └── Reports.razor
│   └── Program.cs
│
└── templates/
    ├── golden_prompts.yaml
    └── config.yaml
```

---

## Quickstart (CLI)

### 1. Prerequisites
- .NET 8 SDK (`dotnet --version` should show 8.x)
- API keys for at least one provider (see table above)

### 2. Clone and Build

```bash
cd project-agile
cp .env.example .env
# Fill in your API keys in .env
dotnet build
```

### 3. Run Your First Evaluation

```bash
# Full evaluation — all models, all metrics
dotnet run --project Agile.Cli -- run \
  --prompts templates/golden_prompts.yaml \
  --config templates/config.yaml \
  --output ./reports

# Bias checks only
dotnet run --project Agile.Cli -- run \
  --mode bias \
  --prompts templates/golden_prompts.yaml \
  --config templates/config.yaml

# Check which models are configured and reachable
dotnet run --project Agile.Cli -- models \
  --config templates/config.yaml --ping
```

### 4. Launch the Blazor Web App

```bash
dotnet run --project Agile.Web
# Open http://localhost:5000
```

---

## Commands Reference

```
agile run         Run an evaluation
agile report      Generate a report from a previous run
agile validate    Validate a prompts or config file
agile models      List configured models and check connectivity
```

See [`docs/spec.md`](./docs/spec.md) for the full command reference.

---

## NuGet Packages

See [`nuget-packages.md`](./nuget-packages.md) for the full package list with explanations.

---

## Documentation

| File | Read When |
| :--- | :--- |
| [`docs/spec.md`](./docs/spec.md) | Full CLI and architecture specification |
| [`docs/metrics_reference.md`](./docs/metrics_reference.md) | Every metric: formula, threshold, failure interpretation |
| [`docs/golden_prompts_guide.md`](./docs/golden_prompts_guide.md) | How to create and maintain the test dataset |
| [`nuget-packages.md`](./nuget-packages.md) | NuGet dependencies and why each is needed |
| [`templates/config.yaml`](./templates/config.yaml) | All configuration options |
| [`templates/golden_prompts.yaml`](./templates/golden_prompts.yaml) | 20 ready-to-use banking test cases |

---

## Regulatory Alignment

| Report Section | EU AI Act / GDPR |
| :--- | :--- |
| Bias + counterfactual results | Art.10 — bias documentation |
| Faithfulness + groundedness | Art.13 — transparency |
| Model comparison matrix | Art.9 — model selection justification |
| Full audit log | Art.12 — record keeping |
