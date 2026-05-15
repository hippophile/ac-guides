## Context

Project AGILE is a Blazor Server app with an existing bias audit pipeline: JSONL datasets → `BiasAuditRunner` → `DecisionBasedScorer` → HTML/CSV reports. All bias audit runs today require a pre-authored dataset file. The Quick Audit flow adds an AI-generation step that produces a dataset in memory from a plain-language role description, then feeds it directly into the existing pipeline.

The app already has: SignalR (Blazor Server ships with it), OpenAI SDK client, `BiasAuditRunner`, `DecisionBasedScorer`, and MudBlazor components.

## Goals / Non-Goals

**Goals:**
- Zero-friction bias audit from a role description (no dataset authoring)
- Live per-dimension progress visible in the UI as each group runs
- Reuse existing runner and scorer unchanged
- In-memory dataset only — no files written to disk

**Non-Goals:**
- Saving generated datasets for reuse (out of scope for POC)
- Support for custom system prompts in this flow
- Non-technical users (no onboarding/wizard layer needed)
- Batch or scheduled quick audits

## Decisions

### D1: GPT-4.1 generates the full dataset in one call, then split by dimension

**Decision:** One GPT-4.1 call returns all test cases as JSONL. Cases are tagged with a `dimension` field (e.g., `"gender"`, `"religion"`). The runner processes them grouped by dimension sequentially so progress can stream per-dimension.

**Alternatives considered:**
- One call per dimension: more controllable but ~11× slower and ~11× the API cost.
- All dimensions in parallel: faster but no meaningful ordering for the live progress feed.

**Rationale:** One-call-then-group balances speed, cost, and UX. The existing runner already groups by `group` field; dimension-tagged JSONL maps naturally onto that.

### D2: Progress via Blazor EventCallback polling, not a dedicated SignalR hub

**Decision:** `QuickAuditRunner` exposes an `OnDimensionComplete` event. The Blazor page subscribes and calls `StateHasChanged()` to push updates to the UI. No new SignalR hub needed.

**Alternatives considered:**
- Dedicated SignalR hub: more decoupled, but overkill for a single-page flow in Blazor Server where the server-client connection is already open.

**Rationale:** Blazor Server runs on the server; `StateHasChanged()` from an event callback is the idiomatic real-time update pattern. Zero new infrastructure.

### D3: GPT-4.1 prompt uses few-shot examples from `demo_loan_bias.jsonl`

**Decision:** The dataset generation prompt includes 3–5 example records from the existing loan dataset to show GPT-4.1 the exact output schema it must produce.

**Rationale:** Few-shot is more reliable than schema-only prompting for structured JSON output. The loan dataset is the richest existing example.

### D4: Bias dimensions are a fixed enum in code, not user-configurable

**Decision:** The 11 dimensions (race, gender, age, religion, income, education, disability, sexuality, pregnancy, immigration, family status) are a `BiassDimension` enum in `Agile.Core`. Users toggle checkboxes but cannot add custom dimensions in the POC.

**Rationale:** Keeps generation prompt deterministic and the scorer's expected variants stable.

## Risks / Trade-offs

- [GPT-4.1 output quality] Generated test cases may not be sufficiently borderline for a given domain → Mitigation: include explicit borderline-profile instructions in the generation prompt; add a quality-check heuristic (reject cases where all variants produce the same decision).
- [Latency] One GPT-4.1 call for 44 cases + 11 sequential runner batches = ~30–60s → Mitigation: show generation progress spinner and stream dimension results as soon as each batch completes so UI never feels frozen.
- [Token cost] 44 test cases × prompt tokens per run ≈ moderate cost → Mitigation: POC uses the model already configured in `agile-settings.json`; no new API key needed.
- [Runner coupling] `BiasAuditRunner` currently expects a file-backed dataset → Mitigation: overload or extend runner to accept `IEnumerable<TestCase>` directly; keep file-backed path intact.

## Open Questions

- Should generated datasets be optionally exportable as JSONL for reuse? (deferred post-POC)
- Should the dimension order in the live table be fixed or sorted by risk score after completion?
