## ADDED Requirements

### Requirement: Run test suite against LLM endpoint
The system SHALL iterate over a list of `TestCase` objects, call the configured `IModelClient` for each, and collect the response into an `EvaluationResult`.

#### Scenario: Successful evaluation run
- **WHEN** `EvalRunner.RunAsync` is called with a dataset and a model client
- **THEN** every test case produces an `EvaluationResult` with `ActualOutput` populated and `LatencyMs` recorded

### Requirement: Record latency per call
The system SHALL measure elapsed time from sending the prompt to receiving the full response and store it in `EvaluationResult.LatencyMs`.

#### Scenario: Latency captured
- **WHEN** a model call completes
- **THEN** `LatencyMs` is greater than zero

### Requirement: Handle model errors gracefully
The system SHALL catch exceptions from the model client, mark the result as failed, and continue to the next test case without aborting the run.

#### Scenario: Model call fails
- **WHEN** the model client throws an exception for one test case
- **THEN** that `EvaluationResult` has `Passed = false` and `Error` populated, and the runner continues with the remaining cases

### Requirement: Configurable delay between calls
The system SHALL support a configurable inter-call delay (milliseconds) to stay within provider rate limits.

#### Scenario: Delay applied between calls
- **WHEN** `DelayBetweenCallsMs` is set to 500
- **THEN** the runner waits at least 500ms between consecutive model calls
