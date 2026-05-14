## ADDED Requirements

### Requirement: CLI validate-token command
The system SHALL provide an `agile validate-token` CLI command that tests the full GitHub token → API chain and prints a pass/fail report for each step.

#### Scenario: Valid token with GitHub Models access
- **WHEN** the user runs `agile validate-token` with a valid GitHub token that has Models access
- **THEN** the command prints a success indicator for the token resolution step and the GitHub Models API call step, and exits with code 0

#### Scenario: Valid token without Copilot subscription
- **WHEN** the user runs `agile validate-token` with a token that lacks the Copilot subscription scope
- **THEN** the command prints success for GitHub Models, failure for the Copilot exchange step with a human-readable message explaining missing scope, and exits with code 1

#### Scenario: No token available
- **WHEN** no `GITHUB_TOKEN` env var is set, `gh auth token` returns empty/error, and `agile-settings.json` has no token
- **THEN** the command prints a clear "No token found" message with instructions to run `gh auth login`, and exits with code 1

### Requirement: Token validation shared service
The system SHALL expose a `TokenValidator` class in `Agile.Core` that can be called programmatically to check token validity without making model inference calls.

#### Scenario: Valid token check
- **WHEN** `TokenValidator.ValidateAsync(token)` is called with a valid GitHub token
- **THEN** it returns a `TokenValidationResult` with `IsValid = true` and the detected scopes

#### Scenario: Invalid token check
- **WHEN** `TokenValidator.ValidateAsync(token)` is called with an expired or invalid token
- **THEN** it returns a `TokenValidationResult` with `IsValid = false` and an error message
