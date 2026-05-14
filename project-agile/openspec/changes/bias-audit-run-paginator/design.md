## Context

The Bias Audit detail page (`BiasVerdictPage.razor`) expands a group row to show per-variant side-by-side cards. Each card currently picks `exampleBase` and `exampleVariant` (the first item in each list). All runs are already available in memory via `currentReport.RawResults`, grouped and ordered when the row expands.

## Goals / Non-Goals

**Goals:**
- Let users step through all N run pairs (base[i] ↔ variant[i]) for each variant card
- Show prompt + response for each run on both sides
- Keep state local to the component — no server round-trips

**Non-Goals:**
- Persisting selected run index across page loads
- Showing runs across different variant groups simultaneously
- Any backend or data model changes

## Decisions

**State: `Dictionary<string, int>` keyed by variant name**
Each variant card needs its own current index. A `Dictionary<string, int> selectedRunIndex` in the `@code` block (initialized to 0 per variant) is the simplest approach. Alternative: a flat `int` shared across all cards — rejected because cards are independent.

**Pairing: index-aligned**
`baseRuns[i]` pairs with `vRuns[i]`. This matches how runs were generated (each counterfactual pair shares the same base prompt structure). Alternative: match by `TestCaseId` — not needed since the runner already generates them in paired order.

**Prompt display: above response, monospace box**
Same visual style as the existing `ActualOutput` box — `font-monospace`, `white-space:pre-wrap`, `font-size:0.75rem`. Prompt gets a subtle label "PROMPT" to distinguish it.

**Paginator placement: card-header, right side**
Next to the existing "N/total runs changed" badge. `[← 1/10 →]` with small icon buttons. Arrow-left/right disabled at boundaries.

## Risks / Trade-offs

- [Index out of range if base and variant run counts differ] → Use `Math.Min(baseRuns.Count, vRuns.Count)` as the bound (already done for mismatch count)
- [State not reset when group collapses] → Acceptable; re-expanding restores last index which is harmless
