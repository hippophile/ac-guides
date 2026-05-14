## MODIFIED Requirements

### Requirement: Token resolution priority
`GitHubModelsClient` SHALL resolve the GitHub token in this order: (1) `GITHUB_TOKEN` environment variable, (2) active `gh auth token` CLI session, (3) `GithubToken` field in `agile-settings.json`. If all sources return empty, it SHALL throw `InvalidOperationException` with a message directing the user to set `GITHUB_TOKEN` or run `gh auth login`.

#### Scenario: Env var takes priority
- **WHEN** `GITHUB_TOKEN` environment variable is set to a non-empty value
- **THEN** `GitHubModelsClient` uses that token and does not invoke `gh auth token` or read settings

#### Scenario: gh CLI fallback
- **WHEN** `GITHUB_TOKEN` is not set AND `gh auth token` returns a valid token
- **THEN** `GitHubModelsClient` uses the CLI token

#### Scenario: Settings as last resort
- **WHEN** `GITHUB_TOKEN` is not set AND `gh auth token` returns empty/fails AND `agile-settings.json` has a non-empty `GithubToken`
- **THEN** `GitHubModelsClient` uses the settings token and logs a warning that it may be stale

#### Scenario: No token available
- **WHEN** all three sources yield empty tokens
- **THEN** `GitHubModelsClient` throws `InvalidOperationException` with message: "No GitHub token found. Set GITHUB_TOKEN or run 'gh auth login'."
