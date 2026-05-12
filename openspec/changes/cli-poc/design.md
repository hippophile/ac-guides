## Context

The scaffold (`Agile.Core`, `Agile.Cli`, `Agile.Web`) exists and builds clean. The golden dataset template (`golden_prompts.yaml`) is already defined with 20 test cases including bias pairs, adversarial cases, and standard QA. The config template (`config.yaml`) defines model endpoints. This change wires evaluation logic into the CLI so the full loop — load dataset → call LLM → score → report — runs from one command.

## Goals / Non-Goals

**Goals:**
- `agile run eval` executes a YAML test suite against a GitHub Models endpoint
- Scores each response: ROUGE-L faithfulness, keyword relevancy, bias consistency (for `bias_test` pairs)
- Produces a JSON run report and renders it as HTML
- `agile models` lists configured endpoints and tests connectivity
- `agile report` renders a saved JSON run to HTML
- All output uses Spectre.Console tables and progress bars

**Non-Goals:**
- Gemini / NVIDIA providers (GitHub Models only for PoC)
- SHAP explainability (Phase 2.5 — requires separate Python integration)
- Blazor UI (Phase 3)
- Authentication / multi-user
- Database persistence (file-based JSON only)

## Decisions

**ROUGE-L for faithfulness, not LLM-as-judge**
LLM-as-judge doubles API costs and adds latency. ROUGE-L is deterministic, free, and sufficient for PoC. Can be upgraded later.

**Keyword overlap for relevancy**
Extract `expected_topics` from the dataset; score the fraction present in the response. Simple, transparent, auditable — matches the bank's regulatory preference for explainable metrics.

**Bias: compare response length + topic coverage across `bias_variants`**
For bias pairs, run base prompt + all variants, then compute: topic coverage delta, response length delta. Flag if any variant scores >10% lower than the base. This is measurable without ground truth comparison.

**Spectre.Console.Cli command tree**
`Agile.Cli` uses `Spectre.Console.Cli` — already a dependency. Commands: `RunEvalCommand`, `ReportCommand`, `ModelsCommand`, `ValidateCommand`.

**Scriban for HTML reports**
Already in the dependency list. Template file at `project-agile/templates/report.html`.

**File layout in Agile.Core**
```
Agile.Core/
├── Datasets/     ← YamlDatasetLoader
├── Runner/       ← EvalRunner (orchestrates calls + scoring)
├── Metrics/      ← FaithfulnessScorer, RelevancyScorer, BiasScorer
└── Reports/      ← RunReportWriter (JSON), HtmlReportRenderer (Scriban)
```

## Risks / Trade-offs

ROUGE-L scores text overlap — not semantic similarity. A correct answer phrased differently scores low. → Mitigation: document this limitation in reports; upgrade to embedding-based similarity in Phase 3.

GitHub Models rate limits (~150 RPM for free tier). Running 20 test cases + 5 bias variants = 25 calls; within limits. → Mitigation: add configurable delay between calls.

`golden_prompts.yaml` has no real context documents (placeholders). Faithfulness scores will be low since the model has no RAG context. → Mitigation: clearly label scores as "no-context baseline" in the PoC report.

## Migration Plan

1. Add `Datasets/`, `Runner/`, `Metrics/`, `Reports/` to `Agile.Core`
2. Replace `Agile.Cli/Program.cs` stub with Spectre command tree
3. Copy `golden_prompts.yaml` to `project-agile/datasets/golden/`
4. Run `agile run eval` against GitHub Models — verify report is generated
5. No rollback needed (greenfield additions only)
