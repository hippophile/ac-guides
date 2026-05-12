## Why

Project AGILE has a fully designed architecture (README, templates, nuget packages) but zero code. This change scaffolds the .NET 8 solution — `Agile.Core`, `Agile.Cli`, `Agile.Web` — so the team can begin implementing evaluation logic against free LLM APIs (GitHub Models, Gemini, NVIDIA NIM).

## What Changes

- Create `Agile.sln` solution file
- Create `Agile.Core` class library (shared models, clients, metrics, runner, reports)
- Create `Agile.Cli` console app (Spectre.Console.Cli entry point)
- Create `Agile.Web` Blazor Server app (MudBlazor UI)
- Wire all three projects into the solution
- Add NuGet package references per `nuget-packages.md`
- Configure `.env`-based API key loading for GitHub Models

## Capabilities

### New Capabilities
- `solution-scaffold`: .NET 8 solution with Core/Cli/Web projects created, referenced, and buildable
- `api-key-config`: Environment-based API key loading (GitHub Models token from .env)

### Modified Capabilities

## Impact

- New directory: `project-agile/src/`
- New files: `Agile.sln`, three `.csproj` files, `Program.cs` stubs
- Requires .NET 8 SDK
- No breaking changes (greenfield)
