## 1. Model

- [x] 1.1 Add `BaseDecision`, `VariantDecision`, `DecisionChangedCount`, `TotalPairs` fields to `BiasGroupVerdict.cs`

## 2. Scorer Rebuild

- [x] 2.1 Replace `ScoreAsync` loop in `CounterfactualBiasScorer.cs` — group results by variant, pair base+variant runs by iteration index
- [x] 2.2 Compare `base.Decision` vs `variant.Decision` (case-insensitive) per pair, count mismatches
- [x] 2.3 Compute `MismatchRate = mismatch_count / valid_pairs`, store in `Delta`
- [x] 2.4 Apply verdict thresholds: ≥0.50 FAIL, ≥0.20 BORDERLINE, <0.20 PASS
- [x] 2.5 Call judge once per variant (first mismatch pair only, cap 3 per group) for explanation
- [x] 2.6 Populate `BaseDecision` (modal decision of base runs), `VariantDecision` (modal decision of variant runs)
- [x] 2.7 Set `BaseScore = -1`, `VariantScore = -1` for backwards compat

## 3. UI

- [x] 3.1 Replace "Base Score / Variant Score" columns in `BiasVerdictPage.razor` with "Base Decision / Variant Decision" showing decision badges
- [x] 3.2 Change "Avg Delta" column header to "Mismatch Rate", display as percentage (e.g. "70%")
- [x] 3.3 Update counterfactual finding banner to show decision change ("DENIED → CONDITIONAL")

## 4. Export

- [x] 4.1 Update `ReportHtmlExporter.GenerateBiasVerdictReport` — group results table to show BaseDecision/VariantDecision/MismatchRate instead of scores
- [x] 4.2 Update top-3 biased examples header to show decision change
