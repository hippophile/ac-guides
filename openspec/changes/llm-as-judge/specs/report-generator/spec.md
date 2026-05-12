## MODIFIED Requirements

### Requirement: Render HTML report from JSON
The system SHALL render a `RunReport` to an HTML file using a Scriban template. When any `EvaluationResult` in the report has `BiasJudgeScore >= 0`, the HTML SHALL include an additional bias pair comparison section below the per-result detail table.

#### Scenario: HTML report generated without judge data
- **WHEN** `HtmlReportRenderer.RenderAsync` is called with a `RunReport` where no results have `BiasJudgeScore >= 0`
- **THEN** an HTML file is produced with no bias pair section (section is conditionally rendered)

#### Scenario: HTML report includes bias pair section when judge data present
- **WHEN** the report contains variant results with `BiasJudgeScore >= 0`
- **THEN** the HTML includes a "Bias Pair Analysis" section grouping base and variant results side-by-side, showing prompt excerpt, response excerpt, ROUGE bias score, judge bias score, bias flag, and judge reasoning for each pair

## ADDED Requirements

### Requirement: Bias pair comparison table in HTML report
The HTML report SHALL include a "Bias Pair Analysis" table that groups each base result with its variants, displaying scores and judge reasoning side-by-side to allow human reviewers to assess equal treatment at a glance.

#### Scenario: Each bias group shows base and all variants as rows
- **WHEN** TC016 has two variants (Mohammed, Maria) and judge was enabled
- **THEN** the bias pair table shows three rows under the TC016 group: base (John), variant 1 (Mohammed), variant 2 (Maria), each with their own judge score and reasoning

#### Scenario: Bias flag visually highlighted
- **WHEN** a variant result has `JudgeResult.BiasFlag = true`
- **THEN** that row is visually highlighted (e.g. amber background) and the flag column shows a warning indicator
