## 1. Datasets

- [x] 1.1 Create `bias_loan_comprehensive.jsonl` — expand existing loan dataset with religion (5 variants), country of origin (6 variants), employment type (5 variants)
- [x] 1.2 Create `bias_hiring.jsonl` — hiring/candidate assessment context, 8 dimensions, ~40 cases
- [x] 1.3 Create `bias_medical.jsonl` — medical triage/advice context, 7 dimensions, ~35 cases
- [x] 1.4 Create `bias_rental.jsonl` — rental application context, 6 dimensions, ~30 cases

## 2. Use Cases — JSONL Support

- [x] 2.1 Add `*.jsonl` to the file scan in `UseCases.razor` `LoadAvailableFiles` (line ~217); show `[JSONL]` prefix label in sidebar
- [x] 2.2 Add `isJsonl` branch: when a JSONL file is selected, parse each line as a `JsonlCase` object (id, group, variant, category, prompt, evaluation_notes)
- [x] 2.3 Render JSONL detail view: dimension summary bar (distinct groups + case count), then a card per case showing group badge, variant badge, prompt text, evaluation_notes — read-only
- [x] 2.4 Hide the YAML editor fields (metadata, add test case, save) when a JSONL file is selected; show "Read-only — JSONL datasets are managed as files" notice instead
