## 1. Fix GitHubModelsClient Token Resolution

- [x] 1.1 Reorder token resolution in `GitHubModelsClient` constructor: env var → `gh auth token` CLI → settings (last resort)
- [x] 1.2 Make token resolution lazy (resolve on first HTTP call, not in constructor) to support async `gh auth token`
- [x] 1.3 Add staleness warning log when falling back to settings token
- [x] 1.4 Throw `InvalidOperationException` with message "No GitHub token found. Set GITHUB_TOKEN or run 'gh auth login'." when all sources are empty

## 2. Fix CopilotModelClient Error Surfacing

- [x] 2.1 Add HTTP status-specific error messages in `GetCopilotSessionTokenAsync`: 401 → auth message, 403 → subscription message
- [x] 2.2 Add auto-retry logic: on 401 from Copilot chat API, clear `_cachedToken` and retry token exchange + request once
- [x] 2.3 Verify `gho_` OAuth token flows through correctly (no code path blocks non-`ghp_` prefix)

## 3. Add TokenValidator Service

- [x] 3.1 Create `src/Agile.Core/Auth/TokenValidator.cs` with `ValidateAsync(string token)` returning `TokenValidationResult { IsValid, Scopes, Error }`
- [x] 3.2 Implement GitHub API `/user` call to verify token validity and parse `X-OAuth-Scopes` header
- [x] 3.3 Add Copilot session exchange check in `TokenValidator` (optional step, graceful failure)

## 4. Add validate-token CLI Command

- [x] 4.1 Create `src/Agile.Cli/Commands/ValidateTokenCommand.cs` implementing `ICommand` (Spectre.Console.Cli)
- [x] 4.2 Wire token resolution chain, call `TokenValidator`, call GitHub Models endpoint with a trivial prompt, report each step pass/fail
- [x] 4.3 Register `validate-token` command in `src/Agile.Cli/Program.cs`

## 5. Add Agile.Tests Project

- [x] 5.1 Create `src/Agile.Tests/Agile.Tests.csproj` with xUnit, NSubstitute, and project reference to `Agile.Core`
- [x] 5.2 Add `Agile.Tests` to `src/Agile.sln`
- [x] 5.3 Write unit tests for `GitHubModelsClient` token resolution (mock env var, mock process, mock settings)
- [x] 5.4 Write unit tests for `TokenValidator.ValidateAsync` using a mock `HttpClient`
- [x] 5.5 Verify `dotnet test src/Agile.sln` runs and passes

## 6. Cleanup

- [x] 6.1 Clear `GithubToken` field in `agile-settings.json` if it contains the old expired PAT
- [x] 6.2 Delete or gitignore `scratch/test_copilot.cs` (contains hardcoded PAT)
- [x] 6.3 Remove the now-empty `Class1.cs` stub from `Agile.Core`
