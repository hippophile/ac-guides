## 1. Dataset Loader

- [x] 1.1 Update `Agile.Core/Models/TestCase.cs` — add `ParentId`, `ExpectedTopics`, `GroundTruth`, `BiasVariants`, `EvaluationNotes` fields
- [x] 1.2 Create `Agile.Core/Datasets/YamlDatasetLoader.cs` — deserialise `golden_prompts.yaml` into `List<TestCase>` using YamlDotNet
- [x] 1.3 Create `Agile.Core/Datasets/DatasetValidationException.cs`
- [x] 1.4 Add schema validation in `YamlDatasetLoader` — throw `DatasetValidationException` if `id`, `prompt`, or `category` are missing
- [x] 1.5 Add category filter support to `YamlDatasetLoader.Load(string path, string? category = null)`
- [x] 1.6 Copy `templates/golden_prompts.yaml` to `datasets/golden/chatbot-sample.yaml`

## 2. LLM Eval Runner

- [x] 2.1 Update `Agile.Core/Models/EvaluationResult.cs` — add `Error`, `GroundTruth`, `ExpectedTopics` fields
- [x] 2.2 Create `Agile.Core/Runner/EvalRunner.cs` — orchestrates: load dataset → call model → collect results → score
- [x] 2.3 Add latency measurement in `EvalRunner` using `Stopwatch`
- [x] 2.4 Add error handling in `EvalRunner` — catch exceptions, set `Passed = false`, continue run
- [x] 2.5 Add `DelayBetweenCallsMs` config property to `EvalRunner`

## 3. Metrics Engine

- [x] 3.1 Create `Agile.Core/Metrics/RougeL.cs` — static `Score(string hypothesis, string reference)` returning F1 [0,1]
- [x] 3.2 Create `Agile.Core/Metrics/RelevancyScorer.cs` — keyword overlap against `ExpectedTopics`
- [x] 3.3 Create `Agile.Core/Metrics/BiasScorer.cs` — compare base + variants, compute max delta
- [x] 3.4 Create `Agile.Core/Metrics/ThresholdEvaluator.cs` — applies pass/fail thresholds, sets `EvaluationResult.Passed`
- [x] 3.5 Wire all scorers into `EvalRunner` — scores written to `EvaluationResult` after each call

## 4. Report Generator

- [x] 4.1 Update `Agile.Core/Models/RunReport.cs` — add `ModelId`, `DatasetPath`, `AverageFaithfulness`, `AverageRelevancy` computed props
- [x] 4.2 Create `Agile.Core/Reports/RunReportWriter.cs` — `SaveAsync(RunReport, string path)` and `LoadAsync(string path)` using Newtonsoft.Json
- [x] 4.3 Create `project-agile/templates/report.scriban.html` — Scriban HTML template with summary table + per-result detail table
- [x] 4.4 Create `Agile.Core/Reports/HtmlReportRenderer.cs` — renders `RunReport` via Scriban template to HTML file
- [x] 4.5 Create `project-agile/reports/` directory (gitignored except `.gitkeep`)

## 5. CLI Commands

- [x] 5.1 Replace `Agile.Cli/Program.cs` with Spectre.Console.Cli `CommandApp` registering all four commands
- [x] 5.2 Create `Agile.Cli/Commands/RunEvalCommand.cs` — settings: `--dataset`, `--model`; shows progress bar; calls `EvalRunner`; prints summary table
- [x] 5.3 Create `Agile.Cli/Commands/ReportCommand.cs` — settings: `--run-id` or `--file`; calls `HtmlReportRenderer`; prints output path
- [x] 5.4 Create `Agile.Cli/Commands/ModelsCommand.cs` — reads `config.yaml`; pings each endpoint; prints connectivity table
- [x] 5.5 Create `Agile.Cli/Commands/ValidateCommand.cs` — settings: `--dataset`; calls `YamlDatasetLoader`; reports errors or success

## 6. Integration & Verification

- [x] 6.1 `dotnet build Agile.sln` — zero errors
- [x] 6.2 Run `agile validate --dataset datasets/golden/chatbot-sample.yaml` — passes clean
- [x] 6.3 Run `agile models` — GitHub Models endpoint shows ✓ reachable
- [x] 6.4 Run `agile run eval --dataset datasets/golden/chatbot-sample.yaml --model gpt-4o-mini-github` — completes, JSON + HTML reports created in `reports/`
- [x] 6.5 Open the HTML report — verify summary table and per-result rows are present
