## Context

The eval runner already collects base + variant `EvaluationResult` objects for bias test pairs and computes a ROUGE-based `BiasScore`. However ROUGE measures token overlap — it cannot detect that two responses differ in warmth, verbosity, or implied helpfulness. The `config.yaml` already defines a `judge:` block with a cross-provider model (Gemini Flash) for exactly this purpose. This change wires the judge into the evaluation pipeline as an optional step.

## Goals / Non-Goals

**Goals:**
- Judge scores tone parity and helpfulness parity across a base prompt and all its bias variants
- Judge output is structured (machine-readable scores + human-readable reasoning)
- Judge runs only when `--judge` flag is passed — zero impact on existing runs
- Judge model is always a different provider than the model under test
- Results are surfaced in the HTML report in a dedicated bias pair section

**Non-Goals:**
- Replacing ROUGE-based `BiasScore` — both scores coexist
- SHAP or explainability beyond the judge's plain-English reasoning
- Automated pass/fail override based on judge score (Phase 3 decision)
- Judging non-bias test cases (adversarial, product inquiry, etc.)

## Decisions

**Structured judge prompt over free-form**
The judge receives a fixed prompt template that asks it to return a JSON object: `{ "tone_delta": 0-1, "helpfulness_delta": 0-1, "bias_flag": true/false, "reasoning": "..." }`. Structured output makes scores machine-readable for reporting and avoids parsing ambiguity. Alternative (free-form prose) was rejected — too hard to extract numeric scores reliably.

**Judge scores stored on the variant result, not the base**
`bias_judge_score` and `judge_reasoning` are written to the variant `EvaluationResult` (the one with `ParentTestCaseId` set). The base result already carries the ROUGE `BiasScore`. This keeps the data model consistent — each result describes its own assessment.

**Judge invoked once per variant, not once per pair**
If TC016 has two variants (Mohammed, Maria), the judge runs twice: base vs Mohammed, base vs Maria. This gives per-variant scores rather than a single group score. More granular and easier to audit.

**Cross-provider judge via existing `GitHubModelsClient`**
The judge model is specified in `config.yaml` under `judge:`. `LlmJudge` reads this config and instantiates the appropriate `IModelClient`. No new client code is needed — `GitHubModelsClient` already accepts arbitrary endpoints and API keys.

**Judge prompt template as a file**
The prompt is stored at `templates/judge_prompt.md` rather than hardcoded. This lets the compliance team review and adjust the scoring criteria without touching code.

## Risks / Trade-offs

Judge model may itself be biased → Mitigation: use a different provider than the model under test; document that judge scores are advisory, not ground truth.

Judge adds API cost and latency (~1 extra call per variant) → Mitigation: opt-in via `--judge` flag; default runs are unaffected.

Structured JSON output may occasionally be malformed → Mitigation: parse defensively; if JSON is invalid, set scores to -1 and flag `judge_error` on the result.

Judge reasoning is qualitative — two runs may produce different text for the same pair → Mitigation: numeric scores are the primary signal; reasoning is for human review only.

## Migration Plan

1. Add `Agile.Core/Judge/` — no changes to existing code until wired in
2. Add fields to `EvaluationResult` (additive, non-breaking)
3. Wire into `EvalRunner` behind `bool enableJudge` parameter
4. Add `--judge` flag to `RunEvalCommand`
5. Update HTML template — new section only renders if any result has `bias_judge_score >= 0`
6. No rollback needed — all changes are additive
