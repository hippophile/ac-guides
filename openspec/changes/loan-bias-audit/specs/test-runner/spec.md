## ADDED Requirements

### Requirement: TestRunner accepts JSONL datasets
The Test Runner page SHALL list `.jsonl` files from the `datasets/golden/` directory alongside existing `.yaml` files in the dataset dropdown.

#### Scenario: JSONL files appear in dataset selector
- **WHEN** `.jsonl` files exist in `datasets/golden/`
- **THEN** they appear in the dataset dropdown with a `[JSONL]` label prefix

### Requirement: Counterfactual mode auto-enabled for JSONL datasets with group fields
When a JSONL dataset is selected and the loader detects that test cases contain `group` and `variant` fields, the Test Runner SHALL automatically enable counterfactual mode and display a notice: "Counterfactual dataset detected — bias audit mode enabled."

#### Scenario: Counterfactual notice shown for paired dataset
- **WHEN** user selects `loan_bias_audit.jsonl`
- **THEN** a notice appears: "Counterfactual dataset detected — bias audit mode enabled"

#### Scenario: Standard JSONL without group fields runs in normal mode
- **WHEN** user selects a `.jsonl` file with no `group` fields
- **THEN** no counterfactual notice is shown and the run proceeds as a standard evaluation

### Requirement: Bias audit run navigates to BiasVerdictReport on completion
When a counterfactual run completes, the Test Runner SHALL navigate to `/bias-verdict/<run-id>` instead of the standard reports page.

#### Scenario: Redirect to bias verdict on counterfactual run completion
- **WHEN** a counterfactual run finishes successfully
- **THEN** the browser navigates to `/bias-verdict/<run-id>`
