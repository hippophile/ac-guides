## ADDED Requirements

### Requirement: `agile run eval` command
The system SHALL implement a `run eval` command that accepts `--dataset <path>` and `--model <name>` arguments, runs the evaluation, and prints a Spectre.Console summary table on completion.

#### Scenario: Successful eval run
- **WHEN** `agile run eval --dataset datasets/golden/chatbot-sample.yaml --model gpt-4o-mini-github` is executed
- **THEN** the CLI prints a progress bar during execution, then a results table, and exits with code 0

#### Scenario: Missing dataset file
- **WHEN** the `--dataset` path does not exist
- **THEN** the CLI prints a red error message and exits with code 1

### Requirement: `agile report` command
The system SHALL implement a `report` command that accepts `--run-id <id>` or `--file <path>`, loads the JSON report, renders HTML, and prints the output path.

#### Scenario: Report rendered
- **WHEN** `agile report --run-id <id>` is executed
- **THEN** HTML file is created and the path is printed to stdout

### Requirement: `agile models` command
The system SHALL implement a `models` command that reads `config.yaml`, lists all configured model endpoints, and sends a single ping prompt to each to verify connectivity.

#### Scenario: Models listed and pinged
- **WHEN** `agile models` is executed
- **THEN** a table is printed with: model name, provider, endpoint, status (✓ reachable / ✗ failed)

### Requirement: `agile validate` command
The system SHALL implement a `validate` command that accepts `--dataset <path>` and validates the file against the golden dataset schema, reporting any errors.

#### Scenario: Valid dataset
- **WHEN** `agile validate --dataset datasets/golden/chatbot-sample.yaml` is executed with a valid file
- **THEN** the CLI prints "Dataset valid — 20 test cases found" and exits 0

#### Scenario: Invalid dataset
- **WHEN** a test case is missing the `prompt` field
- **THEN** the CLI prints the offending case ID and exits 1

### Requirement: Spectre.Console progress display
All long-running commands SHALL display a Spectre.Console `ProgressBar` or `Status` spinner while executing and SHALL NOT output raw log lines during execution.

#### Scenario: Progress shown during eval
- **WHEN** `agile run eval` is running
- **THEN** a progress indicator shows "Running test N of M" updating in place
