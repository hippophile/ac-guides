# CLI Specification: Project AGILE

Full technical specification for the AGILE evaluation CLI. This document defines what the tool does, how it works, and what every command produces.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│                   CLI (click)                       │
│   agile run / agile report / agile validate         │
└───────────────────┬─────────────────────────────────┘
                    │
┌───────────────────▼─────────────────────────────────┐
│                  Runner                             │
│  Loads config → Loads prompts → Dispatches to       │
│  model clients → Collects responses → Sends to      │
│  metrics engine                                     │
└──────┬────────────────────┬───────────────────────┬─┘
       │                    │                       │
┌──────▼──────┐   ┌─────────▼───────┐   ┌──────────▼────┐
│ Model Layer │   │  Metrics Engine  │   │  Judge Layer  │
│ OpenAI      │   │  Faithfulness    │   │  LLM-as-Judge │
│ Anthropic   │   │  Bias            │   │  Scores each  │
│ Ollama      │   │  Relevancy       │   │  response     │
└─────────────┘   │  Cost/Latency    │   └───────────────┘
                  └─────────┬───────┘
                            │
                  ┌─────────▼───────┐
                  │ Report Generator│
                  │ JSON / HTML     │
                  └─────────────────┘
```

---

## Commands

### `agile run`

Runs an evaluation. The core command.

```
Usage: python -m agile run [OPTIONS]

Options:
  --prompts PATH         Path to golden prompts YAML file        [required]
  --config PATH          Path to config.yaml                     [required]
  --output PATH          Directory to write reports to           [default: ./reports]
  --mode TEXT            Evaluation mode: full | bias | benchmark | faithfulness
                         [default: full]
  --models TEXT          Comma-separated list of model names to test.
                         Overrides config.yaml model list.
  --run-id TEXT          Custom run ID. Auto-generated if not provided.
  --no-report            Run evaluation but skip report generation
  --verbose              Print detailed per-question results to console

Examples:
  python -m agile run --prompts prompts.yaml --config config.yaml
  python -m agile run --mode bias --models gpt-4o,gpt-4.1 --prompts prompts.yaml --config config.yaml
```

**What it does:**
1. Loads and validates the config and prompt files
2. For each prompt, sends it to all configured model endpoints in parallel
3. Collects response text, latency, and token counts from each model
4. Sends each response to the judge model for metric scoring
5. Runs bias checks on counterfactual prompt pairs
6. Aggregates results and writes JSON + HTML report

---

### `agile report`

Generates a report from a previous run's JSON results. Useful for reformatting or regenerating after a template change.

```
Usage: python -m agile report [OPTIONS]

Options:
  --run-id TEXT          ID of a previous run                    [required]
  --format TEXT          Output format: html | json | markdown   [default: html]
  --output PATH          Output directory                        [default: ./reports]
  --runs-dir PATH        Directory containing run JSON files     [default: ./reports]
```

---

### `agile validate`

Validates a golden prompts file or config file without running an evaluation. Use this to catch schema errors before a full run.

```
Usage: python -m agile validate [OPTIONS]

Options:
  --prompts PATH         Validate a golden prompts YAML file
  --config PATH          Validate a config.yaml file

Examples:
  python -m agile validate --prompts ./my_prompts.yaml
  python -m agile validate --config ./config.yaml
```

---

### `agile models`

Lists all configured model endpoints and checks connectivity.

```
Usage: python -m agile models [OPTIONS]

Options:
  --config PATH          Config file to read models from         [required]
  --ping                 Send a test request to each model to verify connectivity
```

---

## Evaluation Modes

| Mode | What It Runs |
| :--- | :--- |
| `full` | All metrics: faithfulness, bias, relevancy, groundedness, latency, cost |
| `bias` | Only bias checks: counterfactual consistency, demographic response parity |
| `faithfulness` | Only faithfulness and groundedness against source documents |
| `benchmark` | Only accuracy, latency, and cost — no bias or faithfulness |

---

## Input: Golden Prompts File

See [`templates/golden_prompts.yaml`](../templates/golden_prompts.yaml) for the full structure with examples.

**Minimum required fields per test case:**

```yaml
test_cases:
  - id: "TC001"                          # Unique ID
    category: "product_inquiry"          # Free-form category tag
    prompt: "What is the mortgage rate?" # The question sent to the model
    context_documents:                   # Documents that should be retrieved
      - "mortgage_products_2026.pdf"
    expected_topics:                     # Keywords/topics that must appear in a correct answer
      - "interest rate"
      - "annual percentage rate"
```

**Optional fields:**

```yaml
    bias_variants:                       # Counterfactual pairs for bias testing
      - prompt: "..."
        variant_attribute: "name_signal"
    ground_truth: "The current rate is..." # Exact expected answer (enables exact match scoring)
    evaluation_notes: "..."              # Notes for the human reviewer
    tags: ["compliance", "mortgage"]    # Arbitrary tags for filtering reports
```

---

## Output: Report Structure

Every run produces a JSON file and an HTML report with the following sections:

### 1. Run Metadata
- Run ID, timestamp, models tested, config snapshot, prompt file path and hash

### 2. Executive Summary
- Pass/fail per model per metric
- Overall recommendation: which model to use for this use case and why

### 3. Metrics Comparison Matrix
A table: rows = models, columns = metrics, cells = score + pass/fail flag

### 4. Per-Question Breakdown
For every test case:
- The prompt sent
- Each model's response
- Score per metric per model
- Flagged failures with explanation

### 5. Bias Report
- Counterfactual consistency rate per model
- Any prompt pairs where the decision or response quality differed
- Flagged demographic disparities

### 6. Cost Report
- Tokens used (input + output) per model per run
- Cost per 1,000 tokens (from config pricing table)
- Total cost of the evaluation run
- Projected monthly cost at defined traffic volume

### 7. Audit Log
- Every API call made: timestamp, model, prompt hash, response hash, latency
- This log is immutable — append-only, timestamped

---

## The Judge Model

The judge model is a separate LLM that scores the responses from all tested models.

**Critical rule:** The judge model must not be one of the models under test. If you are testing GPT-4o vs. GPT-4.1, use Claude 3.5 Sonnet as the judge (or vice versa). A model judging its own outputs introduces systematic bias toward its own style.

**What the judge does per response:**
1. Receives: the original prompt, the retrieved context documents, the model's response, and the evaluation criteria
2. Produces: a score (0.0–1.0) for each metric, plus a one-sentence justification for any score below the threshold

**Judge prompt format (internal):**
```
You are an impartial evaluator assessing an AI response in a banking context.

Prompt given to the model: {prompt}
Retrieved context documents: {context}
Model response: {response}

Evaluate the response on the following criteria and return a JSON object:
{
  "faithfulness": <0.0-1.0>,
  "answer_relevancy": <0.0-1.0>,
  "groundedness": <0.0-1.0>,
  "faithfulness_justification": "<one sentence if score < threshold>",
  ...
}
```

---

## Model Client Interface

Every model client implements the same interface:

```python
class ModelClient:
    def complete(
        self,
        prompt: str,
        context: list[str],
        max_tokens: int = 1000,
    ) -> ModelResponse:
        ...

class ModelResponse:
    text: str
    model_name: str
    input_tokens: int
    output_tokens: int
    latency_ms: float
    raw_response: dict
```

This means adding a new model (e.g., Mistral, Gemini) is just a matter of implementing the `ModelClient` interface — the runner and metrics engine don't change.

---

## Error Handling

| Error Type | Behaviour |
| :--- | :--- |
| API key missing | Fail immediately with a clear message before any API calls are made |
| Model API timeout | Retry once after 5 seconds, then mark the response as `TIMEOUT` and continue |
| Model API error (5xx) | Mark response as `ERROR`, log full error, continue with remaining models |
| Prompt validation failure | Fail the specific test case, continue with others, flag in report |
| Judge model failure | Mark all metrics for that response as `UNSCORED`, log, continue |
| All models fail for a prompt | Mark the test case as `FAILED`, include in report as a critical issue |

The tool must never crash silently. Every failure is logged, every test case gets a result (even if the result is an error state).
