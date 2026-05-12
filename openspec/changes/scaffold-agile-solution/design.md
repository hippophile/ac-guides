## Context

Greenfield .NET 8 solution. Architecture is fully specified in `project-agile/README.md` and `nuget-packages.md`. Three projects: `Agile.Core` (class library), `Agile.Cli` (console), `Agile.Web` (Blazor Server). All three free LLM providers (GitHub Models, Gemini, NVIDIA NIM) use the OpenAI-compatible API — one `OpenAIClient` serves all three by swapping base URL and API key.

## Goals / Non-Goals

**Goals:**
- Buildable solution (`dotnet build` passes with zero errors)
- All NuGet references wired per `nuget-packages.md`
- GitHub Models token loaded from `.env` via DotNetEnv
- Stub `Program.cs` in Cli and Web that compiles
- Folder structure matching the README architecture

**Non-Goals:**
- Implementing any evaluation logic
- Gemini or NVIDIA keys (not available yet)
- Tests project (Phase 2)
- CI/CD pipeline

## Decisions

**Single OpenAI client for all providers**
All three providers are OpenAI API-compatible. `OpenAI` .NET SDK v2.1.0 accepts a custom `Endpoint` URI — no custom HTTP clients needed.

**DotNetEnv for key loading**
`DotNetEnv.Env.Load()` reads `.env` at startup. `.env` is gitignored. No secrets in code or config files.

**`Agile.Core` as class library, not NuGet package**
Project reference (`<ProjectReference>`) is sufficient for a monorepo. Packaging adds overhead not needed at this stage.

**Blazor Server, not WASM**
Server-side rendering keeps LLM API calls server-side (keys never leave server). Simpler auth story. MudBlazor is optimised for Blazor Server.

## Risks / Trade-offs

`OpenAI` SDK v2.1.0 is stable but GitHub Models endpoint behaviour may differ from OpenAI spec → Mitigation: isolate provider calls behind `IModelClient` interface from day one so swapping is trivial.

DotNetEnv loads `.env` in dev only; production uses real environment variables → Mitigation: `Env.Load()` is a no-op if file missing, so same code works in both environments.

## Migration Plan

1. Run scaffold commands in `project-agile/src/`
2. Add NuGet packages per `nuget-packages.md`
3. Add `<ProjectReference>` wiring in Cli and Web `.csproj`
4. Add `IModelClient` interface + `GitHubModelsClient` stub in Core
5. Call `DotNetEnv.Env.Load()` at startup in Cli and Web
6. `dotnet build` — must pass clean

No rollback needed (greenfield).
