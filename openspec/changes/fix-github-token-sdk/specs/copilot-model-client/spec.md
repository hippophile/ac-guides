## MODIFIED Requirements

### Requirement: Copilot token exchange error surfacing
`CopilotModelClient` SHALL surface clear, actionable error messages when the GitHub token → Copilot session token exchange fails, distinguishing between auth failures (401/403) and subscription issues (402/404).

#### Scenario: 401 on token exchange
- **WHEN** the token exchange call returns HTTP 401
- **THEN** `CopilotModelClient` throws with message: "GitHub token rejected by Copilot API. Ensure your token has the required Copilot scopes."

#### Scenario: 403 on token exchange (no subscription)
- **WHEN** the token exchange call returns HTTP 403
- **THEN** `CopilotModelClient` throws with message: "GitHub Copilot subscription not found for this account."

#### Scenario: Cached token invalidated mid-session
- **WHEN** a Copilot API call returns HTTP 401 (token expired early)
- **THEN** `CopilotModelClient` clears `_cachedToken`, retries the token exchange once, and retries the original request

#### Scenario: OAuth token accepted
- **WHEN** `GITHUB_TOKEN` or `gh auth token` returns a `gho_` OAuth token that has Copilot access
- **THEN** `CopilotModelClient` successfully exchanges it for a session token and completes the chat request
