## 1. CSV Exporter

- [x] 1.1 Create `src/Agile.Web/Services/ReportCsvExporter.cs` with static `GenerateBiasVerdictCsv(BiasVerdictReport)` method
- [x] 1.2 Write header row: Group, Variant, Decision, BiasScore, Verdict, JudgeReasoning, Prompt, AIResponse
- [x] 1.3 Write one row per RawResult, all fields RFC 4180 quoted
- [x] 1.4 Prepend UTF-8 BOM so Excel opens correctly

## 2. JS Download

- [x] 2.1 Add `downloadCsv(filename, content)` to `wwwroot/app.js` using Blob with `text/csv` MIME type

## 3. UI

- [x] 3.1 Add `ExportCsv()` method to `BiasVerdictPage.razor` calling `ReportCsvExporter.GenerateBiasVerdictCsv`
- [x] 3.2 Add "Export CSV" button next to "Download Report" in detail view
- [x] 3.3 Disable button when `currentReport.RawResults` is empty
