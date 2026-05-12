# AICoE Knowledge Base & Testing Platform Guides

This repository contains the documentation, standards, and planning materials for the **AICoE AI Testing & Governance Platform** at the bank.

The end goal is a system where any AI model deployed at the bank — in credit, fraud, AML, customer service, or any other domain — can be tested, evaluated, and documented to a standard that satisfies regulators, compliance, and business stakeholders.

---

## What's in This Repo

### Foundation Guides

| Guide | What It Covers | Status |
| :--- | :--- | :--- |
| [`basics.md`](./basics.md) | Agentic coding standards for banking AI systems — how to write AI-assisted code that is auditable and safe | ✅ Ready |
| [`bias_testing.md`](./bias_testing.md) | Full bias testing methodology — methodologies, metrics, tools, and the regulator audit checklist | ✅ Ready |
| [`use-case_testing.md`](./use-case_testing.md) | Use-case evaluation framework — how to benchmark models against real banking scenarios | ✅ Ready |
| [`credit_bias_testing_plan.md`](./credit_bias_testing_plan.md) | Step-by-step bias testing plan specifically for AI-assisted credit decision systems | ✅ Ready |

### Planning

| Document | What It Covers | Status |
| :--- | :--- | :--- |
| [`roadmap.md`](./roadmap.md) | Master build roadmap — all phases from foundation through production Blazor app | ✅ Ready |

### Missing Guides (Phase 1 — To Be Written)

| Guide | What It Will Cover |
| :--- | :--- |
| `robustness_testing.md` | Edge case testing, adversarial inputs, concept drift detection |
| `explainability.md` | SHAP, LIME, LLM-generated explanations, GDPR Art.22 format |
| `compliance_reporting.md` | EU AI Act Art.11 documentation structure, report cadence |
| `golden_datasets.md` | How to create and maintain golden datasets without using real PII |
| `monitoring.md` | Production monitoring, drift alerting, feedback loops |

---

## Project AGILE — The First Thing You Build

**[→ Go to Project AGILE](./project-agile/README.md)**

Project AGILE is the first concrete deliverable: a Python CLI tool that automates the evaluation of LLMs in the bank's RAG workflows.

It answers three questions with numbers, not opinions:
1. Is the model biased?
2. Is the expensive model actually better for this task?
3. How often does it hallucinate from bank documents?

Start here if you want to build something immediately.

---

## How to Read This Repo

**If you're starting from scratch:** Read `basics.md` → `roadmap.md` → Project AGILE README.

**If you need to run a bias test on a specific system:** Go to the relevant plan (e.g., `credit_bias_testing_plan.md`) and follow the task sequence.

**If you need to evaluate LLM models:** Go to Project AGILE.

**If a regulator is asking questions:** The deliverables section of each guide lists exactly what documentation to produce.

---

## Technology Stack

| Layer | Technology | Notes |
| :--- | :--- | :--- |
| **CLI Evaluation Tool** | Python 3.11+ | Project AGILE — Phase 2 |
| **Evaluation Framework** | DeepEval | RAG-specific metrics (faithfulness, context precision, etc.) |
| **Explainability** | SHAP | For tree-based models (XGBoost, LightGBM) |
| **Bias Metrics** | Custom implementation | Disparate impact, equal opportunity, counterfactual — implemented from scratch in the CLI |
| **Web Application** | C# / .NET 8 / Blazor | Phase 3 — wraps the CLI logic in a proper UI |
| **Report Format** | HTML + PDF | Generated from each test run, audit-ready |

---

> This is a living document. As guides are completed and the platform is built, this README should be updated to reflect the current state.
