## ADDED Requirements

### Requirement: Score a bias pair for tone and helpfulness parity
The system SHALL accept a base `EvaluationResult` and a variant `EvaluationResult`, send both responses to a configured cross-provider LLM judge, and return a structured `JudgeResult` containing numeric scores and plain-English reasoning.

#### Scenario: Judge scores an identical pair as unbiased
- **WHEN** the base response and variant response are word-for-word identical
- **THEN** `JudgeResult.ToneDelta` is 0.0, `JudgeResult.HelpfulnessDelta` is 0.0, and `JudgeResult.BiasFlag` is false

#### Scenario: Judge flags a significantly colder variant response
- **WHEN** the variant response is noticeably shorter, more formal, or less helpful than the base
- **THEN** `JudgeResult.BiasFlag` is true and `JudgeResult.Reasoning` contains a plain-English explanation

#### Scenario: Judge handles malformed LLM output defensively
- **WHEN** the judge LLM returns output that cannot be parsed as the expected JSON structure
- **THEN** `JudgeResult.ToneDelta` and `JudgeResult.HelpfulnessDelta` are set to -1.0 and `JudgeResult.JudgeError` is populated with the raw output

### Requirement: Use a structured prompt template
The system SHALL load the judge prompt from `templates/judge_prompt.md` and interpolate the base and variant responses before sending to the judge LLM.

#### Scenario: Template loaded at judge construction time
- **WHEN** `LlmJudge` is instantiated with a template path
- **THEN** the template file is read and cached for reuse across multiple scoring calls

#### Scenario: Prompt contains both responses clearly labelled
- **WHEN** the judge prompt is assembled
- **THEN** the base response is labelled "Response A (Base)" and the variant response is labelled "Response B (Variant)" in the prompt text

### Requirement: Judge model must be cross-provider
The system SHALL read the judge model configuration from the `judge:` block in `config.yaml` and SHALL NOT allow the same model ID as the model under test to be used as judge.

#### Scenario: Same model rejected as judge
- **WHEN** the judge model ID matches the model under test ID
- **THEN** `LlmJudge` throws `InvalidOperationException` with message indicating self-evaluation is not permitted
