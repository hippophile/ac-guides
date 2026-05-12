# Bias Testing Plan: AI-Assisted Credit Decision System

**Scope:** Any AI model or system that assists, influences, or automates credit decisions at the bank — including credit scoring models, loan eligibility engines, credit limit calculators, and AI-powered decision-support tools used by credit analysts.

**Goal:** Prove (or disprove) that the AI system does not systematically disadvantage customers based on protected characteristics, and produce the documentation to back that up when regulators ask.

---

## 1. Understand What You're Testing First

Before running a single test, map the system. You need to know exactly where the AI touches the credit decision, because bias can enter at multiple points.

### 1.1 — Map the AI's Role in the Credit Process

Answer these questions about your system:

| Question | Why It Matters |
| :--- | :--- |
| Does the AI make the final decision, or does a human? | Fully automated decisions face stricter GDPR Art.22 requirements |
| What is the AI's output? (score, label, recommendation, ranked list) | Different output types require different bias tests |
| What features does the model use as inputs? | Some inputs are proxies for protected attributes |
| Who sees the AI's output? (customer directly, analyst, manager) | Determines how bias propagates into the actual decision |
| Is the model a scorecard, a tree-based model, or a neural network? | Determines which explainability tools are applicable |

### 1.2 — Identify All Decision Points

Draw the credit decision flow and mark every point where the AI's output influences what happens next:

```
Customer Application
       ↓
[AI: Initial Eligibility Screen]  ← Bias can enter here (application rejection)
       ↓
[AI: Credit Score Calculation]    ← Bias can enter here (score determines product offer)
       ↓
[Analyst Review with AI Support]  ← Bias can enter here (AI recommendation anchors analyst)
       ↓
[AI: Credit Limit Recommendation] ← Bias can enter here (limits vary by group)
       ↓
Final Decision
```

Each point needs to be tested independently.

---

## 2. Define What "Bias" Means for This System

### 2.1 — Protected Attributes in Credit

These are the characteristics the model must not discriminate on — directly or indirectly:

| Attribute | Example of Bias in Credit |
| :--- | :--- |
| **Gender** | Women approved at lower rates than equally qualified men |
| **Age** | Younger or older applicants rejected at higher rates regardless of creditworthiness |
| **Nationality / Country of Origin** | Non-nationals with equivalent profiles receive lower credit limits |
| **Marital Status** | Single applicants penalised vs. married applicants with identical financials |
| **Postcode / Region** | Proxy for ethnicity or income — model may have learned this correlation |
| **Employment Type** | Self-employed vs. salaried penalised beyond what actual risk data justifies |

### 2.2 — Types of Bias to Look For

**Direct Bias:** The protected attribute is directly used as a feature.
- Check: Is gender, age, nationality in the feature list? (Even if "not supposed to be.")

**Proxy Bias:** The model uses a correlated variable instead.
- Check: Does postcode, job title, device type, or application channel correlate strongly with a protected attribute?

**Historical Bias:** The model was trained on past decisions that were themselves biased.
- Check: If human analysts rejected a disproportionate share of a group historically, the model learned to replicate that.

**Measurement Bias:** The outcome variable used in training is not a neutral measure.
- Check: "Default" is measured as missed payments — but if a group was given worse product terms, they may default more due to the terms, not their creditworthiness.

---

## 3. The Testing Plan

### Task 1 — Feature Audit

**What to do:**
1. Obtain the full list of input features the model uses
2. Check if any protected attribute is directly present (even encoded — e.g., "title: Mr/Mrs", date of birth used to derive age)
3. For every remaining feature, calculate its **correlation with each protected attribute** using Cramér's V (categorical) or Pearson/Spearman (numeric)
4. Flag any feature with correlation > 0.3 as a **proxy risk**
5. Consult with the model owner: is the correlation justified by genuine risk signal, or is it spurious?

**Output:** A feature audit table listing each feature, its proxy risk rating, and the justification for keeping or removing it.

---

### Task 2 — Training Data Profiling

**What to do:**
1. Load the training dataset used to build the model
2. Calculate the distribution of each protected attribute group in the training set
3. Calculate the historical approval rate per group in the training labels
4. Flag any group that is underrepresented (< 10% of total) or has a historically anomalous approval rate

**Specific checks:**

| Check | Method |
| :--- | :--- |
| Group representation | Count and percentage per group |
| Approval rate per group | Approved / Total per group |
| Default rate per group | Defaults / Approved per group |
| Feature distribution per group | Compare feature means/medians across groups |

**Output:** A data distribution report. If historical approval rates show strong group disparities, this is evidence of potential historical bias in the training labels — this must be flagged before the model goes live.

---

### Task 3 — Disparate Impact Analysis

**What to do:**

Run the model on a held-out test set. Calculate the approval rate for each group within each protected attribute. Apply the Four-Fifths Rule.

**Formula:**
```
Disparate Impact Ratio = Approval Rate (most disadvantaged group) / Approval Rate (reference group)

Pass threshold: ≥ 0.80
```

**Run this for every protected attribute:**

| Protected Attribute | Groups to Compare |
| :--- | :--- |
| Gender | Female vs. Male (or non-binary if data available) |
| Age | 18–30, 31–50, 51–65, 65+ |
| Nationality | Nationals vs. Non-nationals (or by nationality cluster) |
| Marital Status | Single vs. Married vs. Other |
| Employment Type | Self-employed vs. Employed vs. Unemployed |

**Output:** A disparate impact table — one row per group comparison, with the ratio and a pass/fail flag.

---

### Task 4 — Fairness Metric Suite

**What to do:**

Beyond the four-fifths rule, calculate the full fairness metric suite on the test set. These are the metrics a regulator will ask for.

| Metric | What It Measures | Acceptable Threshold |
| :--- | :--- | :--- |
| **Disparate Impact Ratio** | Relative approval rate across groups | ≥ 0.80 |
| **Equal Opportunity Difference** | Difference in True Positive Rate (qualified applicants approved) across groups | ≤ 0.05 |
| **Predictive Parity Difference** | Difference in Precision (when approved, are we equally confident?) across groups | ≤ 0.05 |
| **Average Odds Difference** | Combined FPR and TPR difference across groups | ≤ 0.05 |
| **Calibration** | Does a predicted score of 0.7 actually correspond to a 70% non-default rate, equally across groups? | Visual calibration curve |

**Tool to use:** `fairlearn` (Python) or `IBM AI Fairness 360 (aif360)`

---

### Task 5 — Counterfactual Testing

**What to do:**

Build a dataset of counterfactual pairs. Take 200–300 real (or realistic synthetic) customer profiles. For each profile, create variants where only one protected attribute changes, everything else stays identical. Run the model on each pair. The decision must not change.

**Example counterfactual pairs:**

```
Profile A: Age=34, Income=42000, CreditScore=680, LTV=72%, Gender=Male   → Score: 720 → APPROVED
Profile B: Age=34, Income=42000, CreditScore=680, LTV=72%, Gender=Female → Score: 695 → APPROVED ✓

Profile A: Age=34, Income=42000, CreditScore=680, LTV=72%, Nationality=National    → APPROVED
Profile B: Age=34, Income=42000, CreditScore=680, LTV=72%, Nationality=Non-National → REJECTED ✗ ← BIAS FLAG
```

**What to measure:**
- **Consistency Rate:** % of counterfactual pairs where the decision is identical
- **Score Shift:** Average absolute difference in credit score between pair variants
- **Decision Flip Rate:** % of pairs where the decision (approved/rejected) changes across the attribute change

**Acceptable thresholds:**
- Consistency Rate ≥ 95%
- Decision Flip Rate ≤ 2%
- Score Shift ≤ 10 points on average

**Output:** A counterfactual test report listing flip rate, consistency rate, and flagged examples where decisions changed.

---

### Task 6 — Slice-Based Performance Analysis

**What to do:**

Do not look only at aggregate model accuracy. Segment the test dataset into demographic slices and measure all performance metrics per slice.

**Slices to evaluate:**

```
Gender × Age Group      → e.g., "Young Women", "Older Men"
Nationality × Employment → e.g., "Non-national, Self-employed"
Age Group × Income Band  → e.g., "18–30, Low Income"
```

For each slice, measure: Accuracy, Precision, Recall, F1, False Positive Rate, False Negative Rate.

**Flag any slice where:**
- Accuracy drops more than 8 percentage points vs. the overall model accuracy
- False Negative Rate (missed approvals for qualified applicants) is more than 10 points above average
- False Positive Rate (unqualified applicants passed through) deviates significantly

**Output:** A slice performance heatmap — rows are slices, columns are metrics. Red cells = flagged.

---

### Task 7 — SHAP Explainability Audit

**What to do:**

Run SHAP on the model to understand which features are driving decisions globally and locally. Use this to catch proxy discrimination.

**Global analysis:**
- Generate a SHAP feature importance plot (mean absolute SHAP values across all predictions)
- Flag any feature in the top 10 that has a high proxy risk rating from Task 1

**Local analysis (per-decision):**
- For each rejected application in the test set, generate the SHAP explanation: which features pushed the score down, by how much
- Verify that rejections are driven by genuine financial risk signals (income, debt-to-income, payment history, LTV) — not by demographic proxies

**For the regulator:**
- Produce a "representative rejection explanation" template — a human-readable summary of why a typical rejection occurred, suitable for customer communication under GDPR Art.22

**Tool:** `shap` Python library (`shap.TreeExplainer` for XGBoost/LightGBM)

---

### Task 8 — Analyst Anchor Bias Check (if AI is advisory, not automated)

If the AI produces a recommendation that a human analyst then reviews, test for **anchoring bias** — the tendency of analysts to follow the AI's recommendation regardless of their own judgment.

**What to do:**
1. Pull historical decisions where the analyst's final decision matched the AI recommendation
2. Pull historical decisions where the analyst overrode the AI
3. Analyse override rates per protected attribute group: are analysts more likely to override in favour of one group over another?
4. Analyse AI recommendation accuracy by group: is the AI systematically underscoring one group, causing analysts to override more often for them?

**Output:** A human-AI alignment report showing override rates and AI accuracy by demographic group.

---

## 4. What You Need Before You Can Test

| Requirement | Details |
| :--- | :--- |
| **Model access** | The model itself (serialized file, API endpoint, or scoring code) |
| **Feature list** | Complete list of all input features and their descriptions |
| **Training dataset** | Or a representative sample with protected attribute labels |
| **Test dataset** | A held-out set with ground truth labels (actual default/non-default outcomes) |
| **Protected attribute data** | Age, gender, nationality for each record — may require matching to customer CRM |
| **Historical decisions** | Past loan decisions with their outcomes (approved/rejected, defaulted/not) |
| **Model documentation** | How was the model trained? What was the intended use? |

---

## 5. Deliverables (What You Hand to the Regulator)

| Document | Contents |
| :--- | :--- |
| **Bias Risk Assessment** | Pre-test assessment of all potential bias vectors identified in Tasks 1 and 2 |
| **Training Data Distribution Report** | Group representation and historical approval rates from Task 2 |
| **Disparate Impact Report** | Four-fifths rule results for all protected attributes from Task 3 |
| **Fairness Metrics Report** | Full metric suite from Task 4, with pass/fail flags |
| **Counterfactual Test Results** | Consistency rate, flip rate, sample flipped examples from Task 5 |
| **Slice Performance Report** | Per-demographic-slice metrics heatmap from Task 6 |
| **SHAP Audit Report** | Global feature importance and sample local explanations from Task 7 |
| **Analyst Alignment Report** | Override rate analysis from Task 8 (if applicable) |
| **Remediation Plan** | For every failed test: what will be done to fix it, by when, and who is responsible |

---

## 6. Task Sequence (What to Do First)

```
1. Map the system and decision flow
        ↓
2. Obtain all required data and model access
        ↓
3. Run the Feature Audit (Task 1) — catch obvious problems early
        ↓
4. Run Training Data Profiling (Task 2) — understand what the model learned from
        ↓
5. Run Disparate Impact Analysis (Task 3) — the primary regulatory test
        ↓
6. Run the full Fairness Metric Suite (Task 4)
        ↓
7. Build the Counterfactual Dataset and run Task 5
        ↓
8. Run Slice-Based Analysis (Task 6)
        ↓
9. Run SHAP Explainability Audit (Task 7)
        ↓
10. Run Analyst Anchor Bias Check if applicable (Task 8)
        ↓
11. Compile all reports into the Regulatory Evidence Package
        ↓
12. Write the Remediation Plan for any failed tests
```

---

## 7. What Happens If a Test Fails

A failed bias test does not necessarily mean you shut down the model. It means you have a documented problem that requires a documented fix. The regulator wants to see that you found it and addressed it — not that everything was perfect.

**Response options for a failed test:**

| Finding | Possible Remediation |
| :--- | :--- |
| Disparate Impact Ratio < 0.80 | Re-train with reweighting or fairness constraints; or adjust decision thresholds per group |
| Proxy feature identified | Remove or transform the feature; re-train and re-test |
| Historical bias in training labels | Apply label correction techniques (e.g., relabel using actual default outcomes only) |
| Counterfactual flip rate > 2% | Investigate which attribute is causing flips; apply post-processing threshold calibration |
| SHAP shows proxy driving rejections | Remove proxy feature from model; assess impact on accuracy; re-train |
| Analyst override pattern favours one group | Human bias training; change UI to reduce anchoring; audit historical decisions |

---

> **Key point:** The tests above are not a one-time exercise. They should be run at deployment, then at a regular cadence (quarterly minimum), and immediately after any model update or significant shift in the customer population. Document every run.
