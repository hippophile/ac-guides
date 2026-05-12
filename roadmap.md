# AICoE AI Testing Platform — Master Roadmap

This is the complete build plan for the AICoE AI Testing & Governance Platform — a system that lets the bank test, validate, and document AI model behaviour for regulatory audits, internal governance, and continuous monitoring.

The end goal is a **Blazor web application** where an AICoE team member can run bias tests, robustness checks, use-case evaluations, and generate regulator-ready audit reports. We get there through a CLI proof of concept first.

---

## The Big Picture

```
Phase 0: Foundation
    ↓
Phase 1: Knowledge Base (Guides)
    ↓
Phase 2: CLI Proof of Concept
    ↓
Phase 3: Blazor Web Application
    ↓
Phase 4: Production Hardening & Compliance Packaging
```

---

## Phase 0: Foundation — Define What We're Building

> Get clarity before writing a single line of code.

### 0.1 — Define All AI Use Cases in Scope
- List every AI system currently in production or under development at the bank
- For each: name, task type, model(s) used, decision impact (advisory vs. automated), regulatory classification (High-Risk / Limited Risk / Minimal Risk under EU AI Act)
- Prioritise by risk level — High-Risk systems go first

### 0.2 — Map the Regulatory Requirements
- Identify applicable regulations for each use case: EU AI Act, GDPR Article 22, EBA Loan Origination Guidelines, PSD2, local central bank requirements
- Define what "compliance" means concretely for each regulation: what evidence is required, in what format, how often

### 0.3 — Define the Testing Dimensions
For every AI system, determine which of the following testing dimensions apply:
- **Bias & Fairness** — Is the model discriminating against protected groups?
- **Robustness** — Does the model degrade gracefully under edge cases, adversarial inputs, or distribution shift?
- **Explainability** — Can every decision be explained to a customer or regulator in plain language?
- **Accuracy & Performance** — Does the model meet the defined performance targets?
- **Compliance** — Does the system's behaviour satisfy the applicable regulatory requirements?

### 0.4 — Define Pass/Fail Thresholds
- For each testing dimension, define the numerical thresholds that separate "pass" from "fail"
- These must be documented, approved by the compliance team, and stored as versioned configuration

### 0.5 — Set Up the Repository Structure
```
aicoe-testing-platform/
├── docs/                    ← The guides (this repo)
├── src/
│   ├── Cli/                 ← Phase 2: CLI tool
│   └── WebApp/              ← Phase 3: Blazor app
├── datasets/
│   └── golden/              ← Golden datasets per use case
├── reports/                 ← Generated audit reports
└── config/
    └── thresholds.json      ← Pass/fail thresholds per use case
```

---

## Phase 1: Knowledge Base — Complete the Guides

> These documents are not just reference material. They are also the specification that the CLI and app will implement.

### 1.1 — Finalise Existing Guides
- [x] `basics.md` — Agentic coding standards (done)
- [x] `bias_testing.md` — Bias testing for banking (done)
- [x] `use-case_testing.md` — Use-case evaluation framework (done)

### 1.2 — Create Missing Guides

#### `robustness_testing.md`
- Define robustness testing for banking AI: what "breaking" looks like for each use case
- Cover: edge case testing, adversarial input testing, distribution shift / concept drift detection, stress testing at scale
- Include: specific robustness tests per use case (credit scoring, fraud, AML, chatbot)

#### `explainability.md`
- Cover global explainability (SHAP, LIME, feature importance) vs. local explainability (per-decision explanation)
- Cover LLM explanation generation: how to use a language model to produce human-readable explanations of ML model decisions
- Regulatory explainability: what format and depth is required for GDPR Article 22 explanations and EU AI Act documentation

#### `compliance_reporting.md`
- Define the structure of an audit report: what goes in, what format, what must be machine-generated vs. human-written
- Cover EU AI Act Article 11 technical documentation requirements
- Define the cadence: when reports are generated (at deployment, quarterly, on demand, after an incident)

#### `golden_datasets.md`
- Guidelines for creating and maintaining Golden Datasets for each use case
- Synthetic data generation: how to create realistic banking data without using real customer PII
- Dataset versioning: how to ensure evaluations are reproducible over time

#### `monitoring.md`
- Production monitoring: how to track model performance, bias metrics, and drift in live systems
- Alerting: when to trigger a human review, when to auto-retrain, when to escalate
- The feedback loop: how production incidents and monitoring findings feed back into the testing platform

### 1.3 — Create an Index / README for the Guides Repo
- A single `README.md` that explains what the repo is, who it's for, how to navigate the guides, and what the platform does

---

## Phase 2: CLI Proof of Concept

> Before building the Blazor UI, prove the testing logic works end-to-end in a CLI. This is the PoC.

**Tech stack:** C# / .NET 8 console application

### 2.1 — Project Setup
- Initialise the .NET CLI project (`dotnet new console`)
- Define the command structure (using `System.CommandLine` or `Spectre.Console.Cli`)
- Define the configuration schema (`thresholds.json`, model config, dataset paths)
- Set up the project solution structure with separate projects for: `Cli`, `Core` (shared logic), `Reports`

### 2.2 — Dataset Loader
- Build a dataset loader that reads Golden Datasets from CSV / JSON
- Validate dataset schema against the expected format for each use case
- Support loading datasets by use-case name: `load-dataset --use-case credit-scoring`

### 2.3 — Bias Testing Module
- Implement Disparate Impact Ratio calculation
- Implement Equal Opportunity Difference calculation
- Implement Counterfactual consistency check (given a dataset of counterfactual pairs, measure output consistency)
- Implement Slice-Based Evaluation (segment dataset by demographic attributes, compute metrics per slice)
- Output: pass/fail per metric against configured thresholds

### 2.4 — Robustness Testing Module
- Implement edge case runner: submit boundary-value inputs and validate output behaviour
- Implement distribution shift detector: compare model performance on "shifted" dataset vs. baseline
- Implement adversarial input tests: pre-defined adversarial examples per use case
- Implement load/latency test for real-time models (fraud detection): measure P50, P95, P99 inference time

### 2.5 — Explainability Module
- Integrate SHAP (via Python subprocess or ML.NET SHAP) for tree-based model explanations
- For LLMs: implement a "chain-of-thought extraction" step — prompt the model to explain its decision and validate that the explanation references the correct features
- Output per decision: top contributing features and their direction (pushes toward approval vs. rejection)

### 2.6 — Use-Case Evaluation Runner
- Implement side-by-side model comparison: run two or more models on the same Golden Dataset and output a comparative metrics table
- Implement LLM evaluation: send prompts to configured model endpoints, collect responses, score against ground truth
- Support model endpoint configuration: OpenAI API, Anthropic API, local Ollama (Llama), Azure OpenAI

### 2.7 — Report Generator
- Generate a structured report in Markdown and HTML from any test run
- Include: test metadata (date, dataset version, model version, thresholds used), results per dimension, pass/fail summary, evidence tables
- Reports are saved to `reports/` with a timestamp and use-case name in the filename

### 2.8 — CLI Command Reference
Define and implement the following commands:
```
aicoe-test run bias --use-case credit-scoring --dataset ./datasets/golden/credit-scoring.csv
aicoe-test run robustness --use-case fraud-detection --dataset ./datasets/golden/fraud.csv
aicoe-test run explainability --use-case credit-scoring --model-path ./models/xgboost.json
aicoe-test run eval --use-case chatbot --models gpt-4o,claude-3.5,llama-3.1 --dataset ./datasets/golden/chatbot.json
aicoe-test report generate --run-id <id> --format html
aicoe-test dataset validate --use-case credit-scoring --file ./my-dataset.csv
```

### 2.9 — CLI PoC Validation
- Run the full bias test on a synthetic credit scoring Golden Dataset
- Run the LLM comparison on a synthetic chatbot Golden Dataset with at least 2 real model endpoints
- Verify the generated HTML report contains all required sections
- Verify pass/fail thresholds are correctly applied and flagged

---

## Phase 3: Blazor Web Application

> Take the CLI logic and wrap it in a proper UI that an AICoE team member (and an auditor) can actually use.

**Tech stack:** C# / .NET 8, Blazor Server (or Blazor WebAssembly — decide based on infrastructure)

### 3.1 — Project Setup
- Initialise the Blazor project
- Set up the shared `Core` library (reuse from CLI)
- Configure authentication (Azure AD / bank SSO)
- Define the navigation structure: Dashboard → Use Cases → Test Runner → Reports → Settings

### 3.2 — Dashboard
- Overview of all registered AI use cases and their current compliance status
- Last test run date, pass/fail status per use case, trend over time
- Quick-action buttons: "Run Tests", "View Last Report", "View Monitoring"

### 3.3 — Use Case Registry
- CRUD for registering AI systems: name, description, model info, regulatory classification, assigned thresholds
- Each use case has a page showing: all past test runs, current status per testing dimension, assigned Golden Dataset

### 3.4 — Test Runner
- UI to configure and launch a test run: select use case, select testing dimensions, select or upload dataset
- Real-time progress feed as tests execute (SignalR for live updates)
- Inline results preview as tests complete

### 3.5 — Results Viewer
- Structured results page per test run: one section per testing dimension
- Bias section: metric cards (DImpact, EO Diff, etc.), slice heatmap, counterfactual table
- Robustness section: edge case results, latency distribution chart
- Explainability section: feature importance chart, per-decision explanation viewer
- LLM Comparison section: side-by-side response table, metric comparison chart

### 3.6 — Report Export
- Generate and download audit reports in PDF and HTML from any test run
- Reports must be suitable for submission to regulators — professional layout, bank branding, clear pass/fail summary
- Include a digital signature or hash for report integrity verification

### 3.7 — Audit Trail
- Immutable log of all test runs: who ran it, when, with which dataset version, with which model version
- Queryable audit history: filter by use case, date range, pass/fail status
- Export audit trail as CSV for regulatory submission

### 3.8 — Settings
- Manage pass/fail thresholds per use case (compliance-team-only access)
- Manage model endpoint configuration (API keys stored in Azure Key Vault)
- Manage dataset library: upload, version, and tag Golden Datasets

---

## Phase 4: Production Hardening & Compliance Packaging

> Make the platform itself audit-ready.

### 4.1 — Production Infrastructure
- Deploy to bank's internal infrastructure (Azure / on-premise)
- Configure role-based access control: AICoE team (full access), Compliance (read + approve), Auditor (read-only)
- Set up database for storing test results, audit trail, and dataset metadata

### 4.2 — Continuous Monitoring Integration
- Connect to production model endpoints to run scheduled bias and drift checks
- Automated alerting when a metric crosses a threshold in production
- Feed results into the dashboard alongside manual test runs

### 4.3 — EU AI Act Technical Documentation Package
- Auto-generate the Article 11 technical documentation for each registered AI system from the platform's data
- Include: system description, intended purpose, risk classification, training data summary, bias test results, explainability documentation, human oversight mechanisms

### 4.4 — Security & Data Privacy Hardening
- Ensure no real customer PII is ever stored in the Golden Datasets (only anonymized / synthetic data)
- Penetration testing on the web application
- Data encryption at rest and in transit
- Audit logging of all admin actions

### 4.5 — Internal Validation & Sign-Off
- Demo the platform to the compliance team and get sign-off on report format
- Demo to legal team to validate EU AI Act documentation coverage
- Train AICoE team on using the platform
- Create an internal user guide

---

## Mini Roadmaps

### Mini Roadmap: Phase 0 (Foundation)
1. List all in-scope AI use cases with regulatory classification
2. Map applicable regulations to each use case
3. Define testing dimensions per use case
4. Define and document pass/fail thresholds
5. Set up the repository structure

### Mini Roadmap: Phase 1 (Guides)
1. Write `robustness_testing.md`
2. Write `explainability.md`
3. Write `compliance_reporting.md`
4. Write `golden_datasets.md`
5. Write `monitoring.md`
6. Write `README.md` for the guides repo

### Mini Roadmap: Phase 2 (CLI PoC)
1. Initialise .NET project and define command structure
2. Build dataset loader and validator
3. Implement bias testing module
4. Implement robustness testing module
5. Implement explainability module
6. Implement use-case evaluation runner (including LLM comparison)
7. Build report generator (Markdown + HTML)
8. Define and wire up all CLI commands
9. Run end-to-end PoC validation on synthetic datasets

### Mini Roadmap: Phase 3 (Blazor App)
1. Initialise Blazor project, set up shared Core library
2. Configure authentication
3. Build use case registry (CRUD)
4. Build dashboard
5. Build test runner with real-time progress
6. Build results viewer (all testing dimensions)
7. Build report export (PDF + HTML)
8. Build audit trail
9. Build settings (thresholds, endpoints, datasets)

### Mini Roadmap: Phase 4 (Production)
1. Deploy to bank infrastructure
2. Configure RBAC
3. Set up continuous monitoring integration
4. Auto-generate EU AI Act Article 11 documentation package
5. Security hardening and penetration testing
6. Internal validation and sign-off with compliance and legal teams
7. Team training and user guide

---

> **Starting Point:** Begin with Phase 0 and Phase 1 in parallel — clarify the scope while completing the missing guides. The CLI PoC (Phase 2) should be the first thing you build and demonstrate to the compliance team, since it validates the testing logic before investing in the UI.
