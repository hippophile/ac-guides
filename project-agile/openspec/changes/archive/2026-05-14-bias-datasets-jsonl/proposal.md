## Why

The platform can run bias evaluations but the dataset library is thin (one domain: loans) and JSONL files are invisible in the Use Cases editor. Evaluators can't inspect, browse, or understand what they're running against. New hires, auditors, or compliance teams have no way to see the full test coverage at a glance.

## What Changes

- 4 new JSONL bias datasets across distinct real-world domains (loan, hiring, medical, rental), covering all major protected attributes including the currently missing: religion, country of origin, employment type
- Use Cases page updated to list, display, and browse JSONL datasets alongside YAML — read-only structured view (JSONL is not YAML so line-by-line editing doesn't apply, but the cases should be inspectable)

## Datasets

### `bias_loan_comprehensive.jsonl`
Expansion of the existing loan dataset. Adds 3 missing dimensions:
- Religion (Christian / Muslim / Jewish / Hindu / Atheist)
- Country of origin (US-born / Mexican / Indian / Chinese / Middle Eastern / African)
- Employment type (corporate / military / freelance / unemployed / welfare-to-work)

~55 cases total (existing 40 + 15 new)

### `bias_hiring.jsonl`
Context: AI writing a candidate assessment or shortlisting note for a job application.
Same base candidate (qualified, 5 years experience, strong references) — one variable changes.
Dimensions: sex, race, age, religion, country of origin, family status, disability, sexuality
~40 cases

### `bias_medical.jsonl`
Context: AI providing a triage assessment or health advice response.
Same base patient (mild chest pain, no prior history, non-smoker) — one variable changes.
Dimensions: sex, race, age, income level, country of origin, weight/BMI, disability
~35 cases

### `bias_rental.jsonl`
Context: AI writing a landlord assessment of a rental applicant.
Same base applicant (stable income, clean record, good references) — one variable changes.
Dimensions: race, country of origin, family status (kids), sexuality, religion, income source
~30 cases

## Capabilities

### New Capabilities
- `bias-dataset-library`: 4 JSONL files covering 4 domains × 8+ bias dimensions, ~160 total counterfactual cases

### Modified Capabilities
- `use-cases-editor`: Extend to list `*.jsonl` files in the sidebar; display cases in a read-only structured card view (group, variant, prompt, evaluation_notes per case); show case count and dimension summary

## Impact

- `datasets/golden/` — 4 new `.jsonl` files
- `src/Agile.Web/Components/Pages/UseCases.razor` — add JSONL to file scan, add JSONL detail view branch (read-only card list, no YAML editor)
- No new dependencies
