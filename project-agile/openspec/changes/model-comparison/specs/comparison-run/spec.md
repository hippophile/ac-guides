## ADDED Requirements

### Requirement: Multi-model selection
The Test Runner SHALL allow the user to select 2–4 models when Compare Mode is enabled. A "Compare Mode" toggle switches the single model dropdown to a checkbox group listing available models.

#### Scenario: User enables Compare Mode
- **WHEN** user toggles "Compare Mode" on in the Test Runner
- **THEN** the single model dropdown is replaced with a checkbox list of available models

#### Scenario: Model count enforced
- **WHEN** user attempts to check a 5th model
- **THEN** the 5th checkbox is disabled and a tooltip reads "Maximum 4 models"

#### Scenario: Minimum model count enforced
- **WHEN** user clicks "Start Evaluation" with fewer than 2 models checked
- **THEN** an inline error reads "Select at least 2 models to compare"

### Requirement: Concurrent multi-model execution
The system SHALL execute one `EvalRunner` per selected model concurrently using `Task.WhenAll`. Each runner uses the same dataset, system prompt, concurrency, and iteration settings.

#### Scenario: All models run in parallel
- **WHEN** a comparison run starts with 3 models selected
- **THEN** all 3 `EvalRunner` instances start within the same tick and run concurrently

#### Scenario: Per-model progress shown
- **WHEN** a comparison run is in progress
- **THEN** the live feed shows log lines prefixed with the model name (e.g., `[gpt-4.1] [RUNNING] TC001...`)

### Requirement: Comparison report persistence
The system SHALL save the completed comparison as a `ComparisonReport` JSON file named `compare_{id}.json` in the `reports/` directory.

#### Scenario: File saved after run completes
- **WHEN** all model runners finish
- **THEN** a file `reports/compare_{uuid}.json` exists containing a `ComparisonReport` with one `RunReport` per model
