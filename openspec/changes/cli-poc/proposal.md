## Why

The solution scaffold exists but has no evaluation logic. Before building the Blazor UI, we need a working CLI that can load a dataset, call a real LLM, score the output, and generate a report — proving the testing loop works end-to-end and giving the compliance team something to validate.

## What Changes

- Implement `agile run eval` command: loads a YAML test suite, calls GitHub Models, scores responses
- Implement dataset loader: reads golden datasets from YAML/JSON
- Implement core metrics: faithfulness, relevancy, bias (demographic parity)
- Implement report generator: outputs JSON + HTML report per run
- Implement `agile report` command: renders a saved run to HTML
- Implement `agile models` command: lists available GitHub Models endpoints
- Replace `Program.cs` stub with Spectre.Console.Cli command tree

## Capabilities

### New Capabilities
- `dataset-loader`: Load and validate YAML/JSON golden datasets for use-case evaluation
- `llm-eval-runner`: Run a test suite against a configured LLM endpoint, collect responses, measure latency
- `metrics-engine`: Score LLM responses — faithfulness (ROUGE-L), relevancy (keyword overlap), bias (demographic parity across prompt variants)
- `report-generator`: Produce a structured JSON run report and render it as an HTML file
- `cli-commands`: Spectre.Console.Cli command tree — `run eval`, `report`, `models`, `validate`

### Modified Capabilities
- `api-key-config`: No requirement changes — implementation only

## Impact

- `Agile.Cli/Program.cs`: replaced with Spectre command tree
- `Agile.Core/`: new folders — `Runner/`, `Metrics/`, `Reports/`, `Datasets/`
- New sample dataset: `project-agile/datasets/golden/chatbot-sample.yaml`
- New report template: `project-agile/templates/report.html` (already exists in templates/)
- No breaking changes to existing stubs
