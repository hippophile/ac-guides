# Use-Case Testing: Real Banking AI Scenarios

This guide covers how to evaluate and benchmark AI models against real banking use cases. The goal is not to find the "best" model in the abstract — it is to find the right model for each specific job, and then prove it works under the conditions that matter in a regulated environment.

---

## 1. The Testing Philosophy

Generic benchmarks (MMLU, HumanEval) don't tell you how a model performs on *your* data, in *your* context, with *your* edge cases. The only meaningful test is an evaluation against a curated dataset of real, production-grade banking scenarios.

**Every use-case test must answer three questions:**
1. **Does it work?** — Accuracy, precision, recall on your actual data
2. **Does it fail safely?** — What happens at the edges? Does it degrade gracefully or catastrophically?
3. **Can you explain it?** — Can you tell a regulator *why* the model made a specific decision?

---

## 2. Banking Use Cases in Scope

### Use Case 1: Credit Scoring & Loan Eligibility

**Task:** Predict probability of default and generate a pass/fail eligibility decision from a customer's financial profile.

**Why it's complex:** This is a **High-Risk AI** use case under the EU AI Act. Every rejection must be explainable to the customer and auditable by regulators.

**Models to compare:**

| Model | Type | Strengths | Weaknesses |
| :--- | :--- | :--- | :--- |
| **XGBoost** | Gradient Boosted Trees | High accuracy on tabular data, SHAP-native explainability | Not suitable for unstructured inputs |
| **Logistic Regression** | Linear Model | Fully interpretable, easy to audit | Lower accuracy on complex feature interactions |
| **LightGBM** | Gradient Boosted Trees | Faster on large datasets, handles missing values natively | Less fairness tooling |
| **GPT-4o (assisted review)** | LLM | Can synthesize unstructured inputs (bank statements as text) | High latency, high cost, harder to audit deterministically |

**Key metrics:** AUC-ROC, KS Statistic, Gini Coefficient, Disparate Impact Ratio, SHAP Feature Importance

**Golden Dataset:** 500+ realistic customer profiles — balanced across income, age, gender, nationality. Include edge cases: self-employed, thin credit files, recent immigrants.

---

### Use Case 2: Fraud Detection

**Task:** Classify financial transactions as legitimate or fraudulent in near-real-time.

**Why it's complex:** Fraud patterns evolve (concept drift). False positives freeze legitimate accounts. False negatives cost money. The model must be fast, adaptive, and fair.

**Models to compare:**

| Model | Type | Strengths | Weaknesses |
| :--- | :--- | :--- | :--- |
| **Isolation Forest** | Anomaly Detection | Unsupervised, good for rare event detection | No probability output, harder to tune |
| **LightGBM** | Supervised Classification | High precision/recall, fast inference | Requires large labeled dataset |
| **LSTM** | Deep Learning / RNN | Captures sequential transaction patterns over time | Slower to train, harder to explain |
| **Graph Neural Network (GNN)** | Deep Learning / Graph | Detects fraud rings via transaction relationship graphs | High infrastructure complexity |

**Key metrics:** Precision @ High Recall, False Positive Rate per demographic group, Inference Latency (P95/P99 < 100ms), Concept Drift Rate, Alert Fatigue Rate

**Golden Dataset:** Real or synthetic transactions covering: card-not-present, account takeover, synthetic identity, money mule activity. Include seasonal patterns and demographic balance.

---

### Use Case 3: AML Alert Triage

**Task:** Classify AML alerts as true positives (suspicious) or false positives (legitimate activity) to reduce analyst workload.

**Why it's complex:** Industry false positive rates are 95%+. Every decision has direct legal consequences — SAR filings, account closures, law enforcement referrals. Explainability is non-negotiable.

**Models to compare:**

| Model | Type | Strengths | Weaknesses |
| :--- | :--- | :--- | :--- |
| **XGBoost + SHAP** | Supervised Classification | Interpretable, analyst-friendly explanations | Requires labeled dataset of analyst decisions |
| **Graph Neural Network** | Deep Learning / Graph | Detects money laundering networks (layering, smurfing) | Very hard to explain to a regulator |
| **Rule-Based + ML Hybrid** | Hybrid | Auditable rule layer, AI reduces false positives | Complex to maintain, two systems to validate |
| **LLM (GPT-4o / Claude 3.5)** | LLM | Can read alert narratives, generate analyst-ready explanations | Non-deterministic, requires extensive output validation |

**Key metrics:** True Positive Rate on confirmed SARs, False Positive Reduction Rate, Explanation Quality Score (human-rated), Time-to-Decision vs. analyst baseline

**Golden Dataset:** Historical alerts labeled by experienced AML analysts. Include SAR-filed and SAR-declined cases, typologies: structuring, layering, trade-based ML, shell companies.

---

### Use Case 4: Customer Service Chatbot (LLM Comparison)

**Task:** LLM-powered assistant handles customer queries. Must be accurate, safe, and consistent regardless of who the customer is.

**Models to compare:**

| Model | Provider | Best For | Key Limitation |
| :--- | :--- | :--- | :--- |
| **GPT-4o** | OpenAI | Complex multi-turn reasoning, broad knowledge | US-hosted — data sovereignty concerns |
| **Claude 3.5 Sonnet** | Anthropic | Long context, strong instruction following, safety alignment | Higher latency at scale |
| **Llama 3.1 70B** | Meta (self-hosted) | On-premise for data sovereignty, cost-efficient at scale | Needs significant infrastructure |
| **Mistral Large** | Mistral AI | EU-hosted, strong multilingual support | Smaller ecosystem |

**Key metrics:** Answer Accuracy, Hallucination Rate, Response Consistency (same question × 10), Demographic Consistency (bias), Safety Refusal Rate, Time to First Token (< 2s for live chat)

**Golden Dataset:** 200+ realistic customer queries across: account inquiries, disputes, product questions, complaint handling. Include adversarial queries and multilingual variants.

---

### Use Case 5: KYC Document Processing

**Task:** Extract and validate information from identity documents (passports, IDs, utility bills) for customer onboarding.

**Models to compare:**

| Model | Type | Strengths | Weaknesses |
| :--- | :--- | :--- | :--- |
| **GPT-4o Vision** | Multimodal LLM | Handles varied formats, multilingual, flexible | Data privacy risk for ID documents sent externally |
| **Azure Document Intelligence** | Managed OCR + ML | Pre-built ID/passport models, on-premise option, built-in audit trail | Less flexible for non-standard documents |
| **Tesseract + Rules** | OCR + Rules | Fully on-premise, auditable, predictable | Poor on low-quality scans, non-Latin scripts |
| **Custom Fine-Tuned Vision Model** | Fine-tuned CNN/ViT | Optimized for your specific document types | Requires labeled data and ongoing maintenance |

**Key metrics:** Field Extraction Accuracy (per field), False Rejection Rate, Demographic Parity in Rejection Rate, Processing Time, Confidence Score Calibration

---

## 3. The Evaluation Experiment Framework

Every use-case test follows this structure:

### Step 1: Define Specific, Measurable Objectives
Don't test for "better quality." Define exact targets:
- "AML false positive rate below 80% (from current 94%)"
- "Disparate Impact Ratio > 0.85 across all gender/age groups"
- "Fraud scoring under 80ms at P99"

### Step 2: Curate a Golden Dataset

| Rule | Why |
| :--- | :--- |
| 100–500 real or realistic examples | Representative of actual production distribution |
| Covers common cases AND hard edge cases | Generalization test |
| Labeled by domain experts, not another model | Ground truth must be trustworthy |
| Versioned and frozen during evaluation | Reproducibility |

### Step 3: Run Side-by-Side Evaluations
Run all candidate models on the exact same Golden Dataset. Use diff-view outputs to see where models diverge on the same input.

- **For LLMs:** Use pairwise human evaluation ("which response is better?")
- **For ML models:** Compare all metrics in a unified table on the same test set

### Step 4: Document the Decision
Record the model selection rationale in writing:
- Why this model over the alternatives
- What the metrics were at selection time
- Known limitations and how they are mitigated
- What ongoing monitoring will be in place

This document is your audit evidence when a regulator asks "why did you choose this model?"

---

## 4. Architecture Reference

### When to Use Which Architecture

| Task | Recommended Architecture |
| :--- | :--- |
| Credit scoring (tabular) | XGBoost / LightGBM (interpretable) |
| Credit scoring (with unstructured features) | TabTransformer |
| Fraud detection (real-time) | LightGBM for latency, LSTM/Transformer for sequence |
| Fraud / AML network detection | Graph Neural Network (GNN) |
| Transaction behavior prediction | JEPA or LSTM |
| Customer service chatbot | GPT-4o / Claude 3.5 / Llama 3.1 (pick based on data sovereignty) |
| Document extraction (KYC) | Azure Document Intelligence or GPT-4o Vision |
| AML alert narrative explanation | LLM + SHAP hybrid |

### JEPA vs. Transformers: Quick Reference

**Transformers** (GPT-4o, Claude, Llama, BERT, TabTransformer):
- Best for text understanding, document processing, customer service, tabular data with mixed feature types
- Explainability: attention weights are not directly interpretable as feature importance

**JEPA (Joint Embedding Predictive Architecture — Meta / LeCun):**
- Best for predicting future states from observed states without pixel/token reconstruction
- Banking fit: transaction behavior forecasting, anomaly detection without labeled data
- Explainability: operates in latent space — outputs are not human-readable without a decoder; harder to audit

---

> **Reminder:** Choosing a model is a decision you will have to defend. Your evaluation methodology, Golden Dataset, metrics, and reasoning are your audit evidence.