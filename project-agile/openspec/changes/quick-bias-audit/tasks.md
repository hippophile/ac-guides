## 1. Core Model — BiasDimension Enum & QuickAuditRequest

- [x] 1.1 Add `BiasDimension` enum to `Agile.Core/Models/` with 11 values: Race, Gender, Age, Religion, Income, Education, Disability, Sexuality, Pregnancy, Immigration, FamilyStatus
- [x] 1.2 Add `QuickAuditRequest` record to `Agile.Core/Models/` with `RoleDescription` (string) and `Dimensions` (List<BiasDimension>)
- [x] 1.3 Add `QuickAuditDimensionResult` record with `Dimension`, `BiasScore` (double), `Status` (enum: Waiting/Running/Complete/Flagged), and `TestCaseCount`

## 2. Dataset Generation Service

- [x] 2.1 Create `Agile.Core/Services/QuickDatasetGenerator.cs` with a single public method `GenerateAsync(QuickAuditRequest, CancellationToken) → Task<List<TestCase>>`
- [x] 2.2 Load 4 few-shot example records from `datasets/golden/demo_loan_bias.jsonl` and embed them in the generation prompt
- [x] 2.3 Build the GPT-4.1 prompt: instruct the model to infer the decision type from `RoleDescription`, produce a borderline profile, and emit one baseline + one variant per selected dimension as JSONL with `id`, `group`, `variant`, `prompt`, `dimension` fields
- [x] 2.4 Call the existing `IModelClient` (use a raw completion, not the eval runner) and parse the response JSONL into `List<TestCase>`, setting `Group` = dimension name and `Variant` to `"base"` or `"variant"`
- [x] 2.5 Register `QuickDatasetGenerator` in `Agile.Web/Program.cs` as a scoped service

## 3. Quick Audit Runner (Orchestrator)

- [x] 3.1 Create `Agile.Web/Services/QuickAuditRunner.cs` that takes a `QuickAuditRequest`, uses `QuickDatasetGenerator` to get test cases, groups them by `dimension`, and runs each group through `EvalRunner` sequentially
- [x] 3.2 Expose `Func<QuickAuditDimensionResult, Task> OnDimensionComplete` callback on `QuickAuditRunner` so the Blazor page can subscribe for live updates
- [x] 3.3 After each dimension group completes, compute bias score using existing `CounterfactualBiasScorer`, build a `QuickAuditDimensionResult`, and invoke the callback
- [x] 3.4 Support `CancellationToken` throughout so the UI Cancel button can stop mid-run

## 4. Blazor Page — Quick Audit UI

- [x] 4.1 Create `Agile.Web/Components/Pages/QuickAudit.razor` at route `/quick-audit`
- [x] 4.2 Add role description `MudTextField` and Run button (`MudButton`); disable Run when input is empty or no dimensions selected
- [x] 4.3 Add dimension checkbox grid using `MudCheckBox` for all 11 `BiasDimension` values with "Select All" / "Clear All" buttons; all checked by default
- [x] 4.4 Add generation phase: show `MudProgressLinear` indeterminate spinner with label "Generating test cases..." while `QuickDatasetGenerator` runs
- [x] 4.5 Add live dimension table: `MudTable` with columns Dimension / Status / Score; render status icons (○ / ⏳ / ✅ / 🔴) and call `StateHasChanged()` inside `OnDimensionComplete` callback to push updates
- [x] 4.6 Add Cancel `MudButton` that cancels the `CancellationTokenSource` and resets UI state
- [x] 4.7 Add post-audit summary section: flagged count, clean count, `MudProgressLinear` score bar per dimension (red if > 0.2), "Export CSV" button, "View Full Report" button (links to existing report page)

## 5. Navigation

- [x] 5.1 Add "Quick Audit" nav item to `Agile.Web/Components/Layout/NavMenu.razor` with route `/quick-audit` and an appropriate MudBlazor icon (e.g., `Icons.Material.Filled.FlashOn`)

## 6. CSV Export for Quick Audit Results

- [x] 6.1 Add `ExportQuickAuditCsv(List<QuickAuditDimensionResult>) → byte[]` method to the existing `Agile.Web/Services/ReportHtmlExporter.cs` (or a new `CsvExporter.cs` if cleaner)
- [x] 6.2 Wire the "Export CSV" button in `QuickAudit.razor` to download the CSV via JS interop (`IJSRuntime.InvokeVoidAsync("downloadFile", ...)`)
