## ADDED Requirements

### Requirement: xUnit test project in solution
The system SHALL include an `Agile.Tests` xUnit project in `Agile.sln` so that `dotnet test src/Agile.sln` discovers and runs tests.

#### Scenario: dotnet test discovers tests
- **WHEN** the user runs `dotnet test src/Agile.sln` from the project root
- **THEN** the command finds at least one test assembly, runs it, and exits with code 0 (all tests pass)

#### Scenario: Test project references Agile.Core
- **WHEN** `Agile.Tests` is built
- **THEN** it compiles against `Agile.Core` without errors, giving access to all public types

### Requirement: Token resolution unit tests
The system SHALL include unit tests covering the token resolution logic in both clients without making real HTTP calls.

#### Scenario: GitHubModelsClient prefers env var over settings
- **WHEN** `GITHUB_TOKEN` is set in the environment AND a token exists in `agile-settings.json`
- **THEN** `GitHubModelsClient` uses the env var token for API calls

#### Scenario: GitHubModelsClient throws on no token
- **WHEN** no token is available from any source
- **THEN** constructing or calling `GitHubModelsClient` throws `InvalidOperationException` with a descriptive message
