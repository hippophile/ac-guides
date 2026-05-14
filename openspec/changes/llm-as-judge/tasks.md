## 1. Judge Models

- [x] 1.1 Create `Agile.Core/Judge/JudgeResult.cs` — fields: `ToneDelta`, `HelpfulnessDelta`, `BiasFlag`, `Reasoning`, `JudgeError`
- [x] 1.2 Create `Agile.Core/Judge/LlmJudge.cs` — constructor takes `IModelClient judge, string modelUnderTestId, string templatePath`; throws if same model ID
- [x] 1.3 Add `ScoreAsync(EvaluationResult base, EvaluationResult variant)` to `LlmJudge` — loads template, interpolates responses, calls judge model, parses JSON response into `JudgeResult`
- [x] 1.4 Add defensive JSON parse in `LlmJudge` — on failure set `ToneDelta = -1`, `HelpfulnessDelta = -1`, populate `JudgeError`
- [x] 1.5 Create `project-agile/templates/judge_prompt.md` — structured prompt asking judge to return `{ "tone_delta": 0-1, "helpfulness_delta": 0-1, "bias_flag": true/false, "reasoning": "..." }`

## 2. EvaluationResult + EvalRunner

- [x] 2.1 Add `BiasJudgeScore` (double, default -1.0), `JudgeReasoning` (string), `JudgeError` (string?) to `Agile.Core/Models/EvaluationResult.cs`
- [x] 2.2 Add `bool enableJudge` and `LlmJudge? Judge` properties to `EvalRunner`
- [x] 2.3 After `BiasScorer.Score()` in `EvalRunner.RunAsync`, loop over variant results and call `Judge.ScoreAsync(baseResult, variantResult)` when `enableJudge` is true
- [x] 2.4 Handle judge exceptions in `EvalRunner` — set `BiasJudgeScore = -1`, populate `JudgeError`, continue run

## 3. CLI

- [x] 3.1 Add `--judge` flag (bool) to `RunEvalSettings` in `RunEvalCommand.cs`
- [x] 3.2 Add `--judge-model` option (string, default reads from `config.yaml judge:` block) to `RunEvalSettings`
- [x] 3.3 Add `--judge-template` option (string, default `templates/judge_prompt.md`) to `RunEvalSettings`
- [x] 3.4 When `--judge` is passed, instantiate `LlmJudge` and assign to `EvalRunner.Judge` before calling `RunAsync`

## 4. HTML Report

- [x] 4.1 Add "Bias Pair Analysis" section to `report.scriban.html` — conditionally rendered when any result has `bias_judge_score >= 0`
- [x] 4.2 Group variant results by `parent_test_case_id` in the template; show base + variants as rows with columns: ID, prompt excerpt, response excerpt, ROUGE bias score, judge score, bias flag, reasoning
- [x] 4.3 Highlight rows where `bias_flag = true` with amber background

## 5. Integration & Verification

- [x] 5.1 `dotnet build Agile.sln` — zero errors
- [x] 5.2 Run `agile run eval --dataset datasets/golden/chatbot-sample.yaml --model gpt-4o-mini --judge` — completes, judge scores populated on variant results in JSON report
- [x] 5.3 Open HTML report — verify "Bias Pair Analysis" section is present with judge reasoning visible
- [x] 5.4 Verify that running without `--judge` produces identical output to existing behaviour (no regression)
