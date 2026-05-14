## Context

The AGILE platform currently resolves GitHub tokens in two clients:
- `GitHubModelsClient`: reads from `agile-settings.json` → `GITHUB_TOKEN` env var → `gh auth token` CLI
- `CopilotModelClient`: reads `GITHUB_TOKEN` env var → `gh auth token` CLI, then exchanges for a Copilot session token

The active `gh` CLI session uses a `gho_` OAuth token (scopes: `gist, read:org, read:user, repo, workflow`). The settings file may contain a stale `ghp_` PAT. Because settings are checked first in `GitHubModelsClient`, a stale token silently overrides the valid active session — causing all model calls to fail with 401.

There is no `Agile.Tests` project, so `dotnet test` exits with "no test projects found."

## Goals / Non-Goals

**Goals:**
- Fix token resolution priority so the active `gh` session is always tried last as a reliable fallback (not the first override)
- Surface clear diagnostics when a token is invalid or lacks required scopes
- Add a `validate-token` CLI command for quick end-to-end token health checks
- Add `Agile.Tests` xUnit project so `dotnet test` works

**Non-Goals:**
- Implementing full OAuth flow or interactive login inside the app
- Supporting non-GitHub model providers
- Changing the Copilot session token exchange endpoint

## Decisions

### Token resolution order for `GitHubModelsClient`
**Decision**: `GITHUB_TOKEN` env var → `gh auth token` CLI → `agile-settings.json` (last resort, with staleness warning)

**Rationale**: Env var is the CI/CD standard; `gh auth token` reflects the developer's active authenticated session; settings token is user-managed and most likely to be stale.

**Alternative considered**: Keep settings-first. Rejected because settings are persistent and silently go stale.

### Clear errors vs. silent fallback
**Decision**: Throw `InvalidOperationException` with a human-readable message when all token sources return empty/null. No silent empty-string usage.

**Rationale**: Silent failures produce confusing 401 errors deep in the HTTP stack. A clear constructor exception points directly to the auth problem.

### `validate-token` as a CLI command
**Decision**: Add `agile validate-token` that tests GitHub Models connectivity, Copilot token exchange, and Copilot chat completion in sequence, reporting each step's pass/fail.

**Rationale**: Developers need a fast, offline-friendly way to diagnose token issues without running a full evaluation.

### xUnit test project
**Decision**: Add `src/Agile.Tests/Agile.Tests.csproj` referencing `Agile.Core` with xUnit + NSubstitute. Add to `Agile.sln`.

**Rationale**: Unit tests for token resolution logic don't require live API calls; NSubstitute mocks `IModelClient`.

## Risks / Trade-offs

- [Risk] `gh auth token` is a subprocess call in constructors → Mitigation: keep it async in `CopilotModelClient` (already is), make `GitHubModelsClient` lazy-resolve token on first HTTP call
- [Risk] `gho_` tokens may not have the Copilot subscription scope → Mitigation: `validate-token` command checks this explicitly and prints scope guidance
- [Risk] Settings token clearing is a one-way write → Mitigation: only clear if the token fails validation (don't blindly wipe it)
