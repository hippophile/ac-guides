## ADDED Requirements

### Requirement: Load YAML golden dataset
The system SHALL load a golden dataset from a YAML file conforming to the `golden_prompts.yaml` schema and deserialise it into a list of `TestCase` objects.

#### Scenario: Valid dataset loads successfully
- **WHEN** a valid YAML file is passed to the loader
- **THEN** all `test_cases` are deserialised with `id`, `prompt`, `expected_topics`, `ground_truth`, and `category` populated

#### Scenario: Bias variants are preserved
- **WHEN** a test case contains a `bias_variants` list
- **THEN** each variant is included as a separate `TestCase` linked to the parent by `ParentId`

### Requirement: Validate dataset schema
The system SHALL validate that required fields (`id`, `prompt`, `category`) are present for every test case and SHALL throw a descriptive error if any are missing.

#### Scenario: Missing required field
- **WHEN** a test case is missing the `prompt` field
- **THEN** the loader throws `DatasetValidationException` with the offending case ID

### Requirement: Filter by category
The system SHALL support filtering test cases by `category` (e.g., `bias_test`, `adversarial`, `product_inquiry`).

#### Scenario: Category filter applied
- **WHEN** the loader is called with `category: "bias_test"`
- **THEN** only test cases with `category == "bias_test"` are returned
