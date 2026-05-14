## ADDED Requirements

### Requirement: Load test cases from JSONL files
The system SHALL parse `.jsonl` files where each line is a JSON object representing one `TestCase`. The loader SHALL skip blank lines and SHALL throw `DatasetValidationException` on malformed JSON. Fields `group` and `variant` are optional strings used for counterfactual pairing.

#### Scenario: Valid JSONL loads successfully
- **WHEN** a `.jsonl` file with valid JSON on each line is passed to `JsonlDatasetLoader.LoadAsync()`
- **THEN** the loader returns a `List<TestCase>` with one entry per non-blank line

#### Scenario: Malformed line throws validation exception
- **WHEN** any line in the JSONL file is not valid JSON
- **THEN** `DatasetValidationException` is thrown with the line number and file path in the message

#### Scenario: Missing required fields throw validation exception
- **WHEN** a line is missing the `id` or `prompt` field
- **THEN** `DatasetValidationException` is thrown identifying the missing field and line number

#### Scenario: Blank lines are skipped
- **WHEN** the JSONL file contains blank lines
- **THEN** those lines are ignored and do not produce test cases

### Requirement: Auto-detect format by file extension
The system SHALL route `.yaml` and `.yml` files to `YamlDatasetLoader` and `.jsonl` files to `JsonlDatasetLoader` via a `DatasetLoaderFactory` that selects the correct loader by extension.

#### Scenario: JSONL extension routes to JSONL loader
- **WHEN** `DatasetLoaderFactory.GetLoader("loan_bias_audit.jsonl")` is called
- **THEN** it returns an instance of `JsonlDatasetLoader`

#### Scenario: YAML extension routes to YAML loader
- **WHEN** `DatasetLoaderFactory.GetLoader("loans.yaml")` is called
- **THEN** it returns an instance of `YamlDatasetLoader`

#### Scenario: Unknown extension throws
- **WHEN** `DatasetLoaderFactory.GetLoader("data.csv")` is called
- **THEN** an `ArgumentException` is thrown stating the format is unsupported
