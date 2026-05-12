## Why

The current ROUGE-L + keyword overlap metrics prove factual consistency but cannot detect tone, warmth, or helpfulness disparities across demographic variants — the exact type of subtle bias that regulators (EU AI Act, consumer protection law) hold banks accountable for. An LLM-as-judge layer is needed to score bias pairs on equal treatment, not just equal content.

## What Changes

- Add `Agile.Core/Judge/` — new judge client that sends a base + variant response pair to a second LLM and returns structured bias scores
- Add `bias_judge_score` and `judge_reasoning` fields to `EvaluationResult`
- Add structured judge prompt template to `templates/`
- Add `--judge` flag to `agile run eval` to enable judge scoring per run
- Add side-by-side bias pair section to the HTML report showing judge verdicts
- Judge model is always a different provider than the model under test (configured via `config.yaml` `judge:` block, already present)

## Capabilities

### New Capabilities
- `llm-judge`: Send base + variant response pairs to a cross-provider LLM judge; receive structured scores for tone parity, helpfulness parity, and a bias flag with plain-English reasoning

### Modified Capabilities
- `llm-eval-runner`: Add optional judge invocation after bias pair results are collected; populate `bias_judge_score` and `judge_reasoning` on affected `EvaluationResult` objects
- `report-generator`: Add side-by-side bias pair comparison table to HTML report showing judge score, flag, and reasoning per pair

## Impact

- `Agile.Core/Judge/LlmJudge.cs` — new file
- `Agile.Core/Judge/JudgeResult.cs` — new file (structured judge output)
- `Agile.Core/Models/EvaluationResult.cs` — two new fields
- `Agile.Core/Runner/EvalRunner.cs` — optional judge invocation post-bias-scoring
- `Agile.Cli/Commands/RunEvalCommand.cs` — `--judge` flag
- `project-agile/templates/judge_prompt.md` — structured judge prompt template
- `project-agile/templates/report.scriban.html` — new bias pair section
- No breaking changes to existing metrics or report structure
