## ADDED Requirements

### Requirement: Infer decision type from role description
The `QuickDatasetGenerator` service SHALL send the user's role description to GPT-4.1 and infer the decision type (approve/deny, score, rank, recommend) without requiring a system prompt from the user.

#### Scenario: Role description processed
- **WHEN** the service receives a role description string and a list of selected dimensions
- **THEN** it SHALL produce a structured prompt that instructs GPT-4.1 to generate JSONL test cases for a borderline applicant profile with one variant per selected dimension

### Requirement: Borderline profile generation
The generated dataset SHALL contain exactly one borderline profile group per selected dimension. Each group SHALL have a neutral baseline variant plus one demographically-signaled variant. All variants in a group SHALL be identical except for the demographic signal.

#### Scenario: Borderline cases produced
- **WHEN** generation completes successfully
- **THEN** the result SHALL contain at minimum 2 records per selected dimension (baseline + 1 variant)

#### Scenario: Dimensions tagged in output
- **WHEN** the JSONL is parsed
- **THEN** each record SHALL include a `dimension` field matching one of the selected dimension enum values

### Requirement: In-memory dataset only
The generated dataset SHALL never be written to disk. It SHALL be returned as `IEnumerable<TestCase>` to the caller.

#### Scenario: No files created
- **WHEN** generation completes
- **THEN** no JSONL files SHALL appear in the `datasets/` directory

### Requirement: Few-shot prompt construction
The generation prompt SHALL include 3–5 example records from `datasets/golden/demo_loan_bias.jsonl` as few-shot examples to anchor the output schema.

#### Scenario: Schema-conformant output
- **WHEN** GPT-4.1 returns the generated cases
- **THEN** all records SHALL deserialize into `TestCase` objects without error
