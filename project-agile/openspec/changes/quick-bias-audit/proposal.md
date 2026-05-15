## Why

Running a bias audit today requires a manually crafted JSONL dataset, which blocks non-dataset authors from testing their AI roles. A "Quick Audit" flow lets any technical user test any AI role for bias across all protected dimensions in seconds, with zero dataset prep.

## What Changes

- New Blazor page `/quick-audit` with a role-description input and bias-dimension checkboxes
- New `QuickDatasetGenerator` service: calls GPT-4.1 to produce an in-memory borderline counterfactual dataset from a plain-language role description
- Real-time per-dimension progress feed via SignalR (○ Waiting → ⏳ Running → ✅ / 🔴)
- Post-audit summary view with per-dimension scores, export, and link to full report
- New "Quick Audit" nav item in the Blazor sidebar

## Capabilities

### New Capabilities

- `quick-audit-ui`: The Blazor page, sidebar nav entry, dimension checkbox grid, role input, and run button
- `quick-dataset-generation`: GPT-4.1 service that infers decision type from a role description and generates a JSONL borderline counterfactual dataset in memory for all selected bias dimensions
- `live-audit-progress`: SignalR hub and Blazor component showing per-dimension live status updates as the existing runner processes each group

### Modified Capabilities

## Impact

- `Agile.Web`: new page, nav item, SignalR hub, and UI components
- `Agile.Core`: new `QuickDatasetGenerator` service; existing `BiasAuditRunner` and scorer consumed unchanged
- No new dependencies — uses existing OpenAI SDK client and SignalR (already in Blazor Server)
- No schema or dataset file changes
