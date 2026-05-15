## Context

BiasVerdictReport now stores `RawResults` (List<EvaluationResult>) with full per-case data. The PDF export covers presentation. A CSV export gives stakeholders machine-readable data for Excel/Sheets analysis.

## Goals / Non-Goals

**Goals:**
- One-click CSV download from Bias Audit detail page
- Every raw result row: group, variant, run index, decision, bias score, verdict, prompt, AI response, judge reasoning
- No server-side dependencies — generate CSV string in C#, download via JS blob

**Non-Goals:**
- CSV export for RunReport or ComparisonReport (out of scope for now)
- Server-side file storage of CSV
- Filtering or column selection UI

## Decisions

- **New static method `ReportCsvExporter.GenerateBiasVerdictCsv(BiasVerdictReport)`** rather than adding to `ReportHtmlExporter` — keeps concerns separate, easier to extend later
- **Client-side download via `downloadCsv` JS function** — consistent with existing `printAsPdf` pattern, no server route needed
- **Quote all fields** — prompt and AI response contain commas, newlines; RFC 4180 quoting required
- **UTF-8 BOM** — ensures Excel opens the file with correct encoding without user intervention

## Risks / Trade-offs

- Long prompt/response text makes rows wide — unavoidable, data is what it is
- Old reports with empty `RawResults` will export a CSV with headers only — acceptable, show a note in UI
