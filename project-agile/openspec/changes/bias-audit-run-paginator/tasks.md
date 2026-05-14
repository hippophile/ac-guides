## 1. State

- [x] 1.1 Add `Dictionary<string, int> selectedRunIndex` field to the `@code` block in `BiasVerdictPage.razor`
- [x] 1.2 Add `PrevRun(string variant, int max)` and `NextRun(string variant)` helper methods that clamp index within `[0, max-1]`
- [x] 1.3 Reset `selectedRunIndex` in `ToggleGroup` when a group is collapsed (or on expand)

## 2. Paginator UI

- [x] 2.1 In the variant card header, replace the static mismatch badge area with a flex row: paginator on the left, mismatch badge on the right
- [x] 2.2 Render `[← Run N/total →]` controls using `<button>` elements bound to `PrevRun`/`NextRun`, disabled at boundaries

## 3. Per-run panel content

- [x] 3.1 Replace `exampleBase` / `exampleVariant` lookups with indexed access: `baseRuns[idx]` and `vRuns[idx]` where `idx = selectedRunIndex.GetValueOrDefault(variantName, 0)`
- [x] 3.2 In the base panel: show the single decision badge for `baseRuns[idx].Decision`, then a "PROMPT" label + prompt text box, then a "RESPONSE" label + actual output box
- [x] 3.3 In the variant panel: same structure using `vRuns[idx]`
- [x] 3.4 Remove the aggregate distribution badge loops (or move them to a summary row above the paginator) since per-run view replaces them
