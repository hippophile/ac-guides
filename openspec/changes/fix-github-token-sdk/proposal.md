## Why

The GitHub token resolution chain in `GitHubModelsClient` and `CopilotModelClient` is broken: stale PAT tokens in `agile-settings.json` override the active `gh` CLI session, and the Copilot internal token exchange is untested/unreliable — causing all test runs to fail with auth errors.

## What Changes

- Remove stale/hardcoded token from `agile-settings.json` and clear it from `SettingsManager` defaults
- Fix `GitHubModelsClient` token resolution to prefer the live `gh auth token` over a potentially stale settings token, falling back to `GITHUB_TOKEN` env var
- Fix `CopilotModelClient` token exchange to correctly handle `gho_` OAuth tokens and surface clear error messages when the Copilot subscription scope is missing
- Add a `models validate-token` CLI command that tests the full token → API round-trip and reports what's working and what's not
- Add an xUnit test project `Agile.Tests` to the solution so `dotnet test` works

## Capabilities

### New Capabilities
- `token-validation`: CLI command and shared validation logic to verify GitHub token validity against both GitHub Models and Copilot endpoints, with human-readable diagnostics
- `test-project`: xUnit test project (`Agile.Tests`) added to the solution covering token resolution logic and client construction

### Modified Capabilities
- `github-models-client`: Token resolution order fixed — live `gh auth token` takes priority over settings; empty/invalid tokens rejected with clear error
- `copilot-model-client`: `gho_` OAuth token support verified; Copilot session exchange errors surfaced clearly; stale static cache cleared on 401

## Impact

- `src/Agile.Core/Clients/GitHubModelsClient.cs` — token resolution logic
- `src/Agile.Core/Clients/CopilotModelClient.cs` — token exchange error handling
- `src/Agile.Cli/Commands/` — new `ValidateTokenCommand`
- `src/Agile.sln` — add `Agile.Tests` project
- `agile-settings.json` — clear stale token field
