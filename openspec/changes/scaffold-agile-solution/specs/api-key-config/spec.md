## ADDED Requirements

### Requirement: API keys load from environment
The system SHALL read `GITHUB_TOKEN` and `GITHUB_MODELS_ENDPOINT` from environment variables at startup. It SHALL NOT hardcode credentials in source files.

#### Scenario: Token available at runtime
- **WHEN** application starts with `GITHUB_TOKEN` set in environment or `.env`
- **THEN** `Environment.GetEnvironmentVariable("GITHUB_TOKEN")` returns the token value

### Requirement: .env file loads in development
The CLI and Web app SHALL call `DotNetEnv.Env.Load()` at startup so developers can store keys in `.env` without setting system environment variables.

#### Scenario: .env loaded on startup
- **WHEN** `.env` file exists in the working directory
- **THEN** its key-value pairs are available via `Environment.GetEnvironmentVariable`

#### Scenario: Missing .env is not an error
- **WHEN** no `.env` file exists (e.g., production environment)
- **THEN** application starts normally using system environment variables

### Requirement: .env is gitignored
The `.env` file SHALL be listed in `.gitignore` and SHALL NOT be committed to the repository.

#### Scenario: Git does not track .env
- **WHEN** developer runs `git status` after creating `.env`
- **THEN** `.env` does not appear in tracked or untracked files
