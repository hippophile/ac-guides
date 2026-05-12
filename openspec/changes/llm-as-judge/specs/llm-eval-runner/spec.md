## MODIFIED Requirements

### Requirement: Run test suite against LLM endpoint
The system SHALL iterate over a list of `TestCase` objects, call the configured `IModelClient` for each, and collect the response into an `EvaluationResult`. When `enableJudge` is true, the runner SHALL additionally invoke `LlmJudge` for each bias variant result and populate `BiasJudgeScore` and `JudgeReasoning` on that result.

#### Scenario: Successful evaluation run without judge
- **WHEN** `EvalRunner.RunAsync` is called with `enableJudge: false`
- **THEN** every test case produces an `EvaluationResult` with `ActualOutput` populated, `LatencyMs` recorded, and `BiasJudgeScore` left at -1.0

#### Scenario: Judge invoked for each bias variant when enabled
- **WHEN** `EvalRunner.RunAsync` is called with `enableJudge: true` and the dataset contains bias pairs
- **THEN** for each variant result (where `ParentTestCaseId` is not empty), `LlmJudge.ScoreAsync` is called with the base result and that variant result, and `BiasJudgeScore` and `JudgeReasoning` are populated on the variant result

#### Scenario: Judge failure does not abort the run
- **WHEN** `LlmJudge.ScoreAsync` throws an exception for one variant
- **THEN** that variant result has `BiasJudgeScore` set to -1.0 and `JudgeError` populated, and the runner continues with remaining test cases

## ADDED Requirements

### Requirement: Store judge scores on EvaluationResult
The system SHALL add `BiasJudgeScore` (double, default -1.0), `JudgeReasoning` (string), and `JudgeError` (string?) fields to `EvaluationResult`.

#### Scenario: Default state before judge runs
- **WHEN** an `EvaluationResult` is created and judge is not enabled
- **THEN** `BiasJudgeScore` is -1.0 and `JudgeReasoning` is empty string
