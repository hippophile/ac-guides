## Why

Banks deploying LLMs in lending workflows face legal exposure if those models treat applicants differently based on protected characteristics. There is no way today to run a structured, evidence-based bias audit on GPT-4.1 across all legally protected attributes and produce a verdict that compliance teams can act on.

## What Changes

- New JSONL dataset format support in `Agile.Core` for scalable, machine-native test case storage
- A real counterfactual bias dataset: identical financial profiles, one protected attribute changed per variant (~60-80 test cases covering sex, race, age, disability, sexuality, family status, location, income signal)
- Pair-aware bias scorer that compares base vs. counterfactual responses and computes a delta per protected attribute group
- Verdict engine: delta threshold → PASS / FAIL / BORDERLINE + auto-generated "why" sentence per group
- New Blazor page: **Bias Verdict Report** — per-attribute group table showing verdict, delta score, and evidence sentence
- Wire the full pipeline into the existing `EvalRunner` and report persistence layer

## Capabilities

### New Capabilities
- `jsonl-dataset-loader`: Load and validate test cases from `.jsonl` files; each line is a `TestCase` JSON object with a `group` and `variant` field for counterfactual pairing
- `counterfactual-bias-scorer`: Pair-aware scorer that groups results by `(id_base, id_variant)`, computes LLM-as-judge score delta, and emits a `BiasGroupVerdict` (PASS/FAIL/BORDERLINE) with evidence
- `bias-verdict-report`: Blazor page and backing report model that renders per-attribute group verdicts, deltas, and why-sentences in a compliance-ready table
- `loan-bias-dataset`: The actual `.jsonl` dataset — 8 protected attribute groups × ~8 counterfactual pairs, loan officer assessment scenario, GPT-4.1 as the model under test

### Modified Capabilities
- `test-runner`: Extend to accept `.jsonl` datasets alongside existing `.yaml`; surface counterfactual mode toggle when a JSONL dataset is selected

## Impact

- `src/Agile.Core/Datasets/` — new `JsonlDatasetLoader.cs`
- `src/Agile.Core/Metrics/` — new `CounterfactualBiasScorer.cs`
- `src/Agile.Core/Models/` — new `BiasGroupVerdict.cs`, `BiasVerdictReport.cs`
- `src/Agile.Core/Reports/` — new `BiasVerdictReportWriter.cs`
- `src/Agile.Web/Components/Pages/` — new `BiasVerdictReport.razor`
- `src/Agile.Web/Components/Layout/NavMenu.razor` — add Bias Verdict link
- `datasets/golden/` — new `loan_bias_audit.jsonl`
- No new NuGet dependencies required
