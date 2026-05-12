# AI Bias Testing for Banking Systems

When a regulator asks "is your AI biased?" — this guide is how you answer them with evidence, not promises.

Bias in AI doesn't always look like a broken algorithm. It often appears as a perfectly functioning model that happens to systematically disadvantage a protected group of customers. In banking, this is both a legal problem and a reputational one.

---

## 1. Why This Matters for a Bank

### The Regulatory Reality

Under the **EU AI Act**, AI systems used for credit scoring, insurance pricing, and financial decisions are classified as **High-Risk AI**. This means you are legally required to:

- Perform and document bias risk assessments
- Demonstrate that outputs are not discriminatory
- Maintain logs that regulators can audit at any time

Under **GDPR Article 22**, customers have the right to not be subject to solely automated decisions with significant legal effects — and the right to a human review if they request one.

The **EBA Guidelines on Internal Governance** and the **EBA Guidelines on Loan Origination and Monitoring** further require that credit decision models are fair, explainable, and regularly validated.

### The Business Reality

Beyond regulation: a biased model that denies loans disproportionately to certain demographics can trigger:
- Regulatory fines (EU AI Act fines reach up to €30M or 6% of global turnover)
- Class action lawsuits
- Press exposure and brand damage
- Forced model shutdown mid-production

Bias testing is not ethical decoration. It is risk management.

---

## 2. What We Mean by "Bias" in a Banking Context

### Protected Attributes

These are the characteristics that a model must not discriminate on, explicitly or implicitly:

| Attribute | Banking Example |
| :--- | :--- |
| **Gender** | A credit scoring model that approves men at higher rates than equally-qualified women |
| **Age** | A churn model that flags elderly customers as "high risk" based on age alone |
| **Nationality / Ethnicity** | A fraud detection model with higher false-positive rates for foreign-sounding names |
| **Marital Status** | A loan model that penalizes single applicants vs. married applicants with identical financials |
| **Postcode / Region** | A pricing model that uses postcode as a proxy for race or income (proxy discrimination) |

> **Important:** You don't have to use protected attributes directly for bias to occur. A model can learn to use a proxy variable (e.g., postcode, school name, shopping behavior) that correlates with a protected attribute. This is called **proxy discrimination** and it is just as illegal.

---

## 3. Testing Methodologies

### 3.1 Disparate Impact Analysis (The Four-Fifths Rule)

**What it is:** Compare the pass/approval rate of a protected group against the majority group. If the protected group's rate is less than 80% of the majority group's rate, the model has disparate impact.

**Formula:**
```
Disparate Impact Ratio = (Approval Rate: Protected Group) / (Approval Rate: Reference Group)

If ratio < 0.80 → Statistically significant bias
```

**Banking Example:**
- Women approved for personal loans: 61%
- Men approved for personal loans: 78%
- Ratio: 61 / 78 = 0.78 → **Below 0.80 — bias flag triggered**

**Tool to use:** IBM AI Fairness 360 (`aif360`), Fairlearn (`fairlearn`)

---

### 3.2 Counterfactual Testing

**What it is:** Take a real customer record. Change only one protected attribute (e.g., change gender from Male → Female, or change nationality). Run the model again. The decision should not change.

**Why it's powerful:** It isolates the exact causal contribution of a protected attribute to the model's output. If flipping gender changes the loan decision — the model is using gender.

**Banking Example:**
```
Input A: Age=35, Income=45000, CreditHistory=Good, Gender=Male → APPROVED
Input B: Age=35, Income=45000, CreditHistory=Good, Gender=Female → REJECTED
→ Gender is causal → Bias confirmed
```

**How to run it at scale:** Build a "counterfactual dataset" — duplicate every test case with all protected attribute variants — and measure the consistency of outputs across variants.

---

### 3.3 Accuracy Parity Audits

**What it is:** Measure whether your model's performance metrics (accuracy, false positive rate, false negative rate) are equal across demographic groups.

**The three fairness metrics that matter most for banking:**

| Metric | What It Measures | Banking Risk |
| :--- | :--- | :--- |
| **Equal Opportunity** | Same True Positive Rate across groups | Are qualified applicants from all groups approved equally? |
| **Predictive Parity** | Same Precision across groups | When you approve someone, are you equally confident regardless of their group? |
| **Equalized Odds** | Same TPR and FPR across groups | The strictest standard — used for high-stakes decisions |

> **For fraud detection in particular:** A model with a high False Positive Rate for a specific nationality is flagging innocent customers from that group at a disproportionate rate — this is discriminatory even if the overall accuracy looks fine.

---

### 3.4 Slice-Based Evaluation

**What it is:** Segment your test dataset into demographic "slices" and measure model performance on each slice separately, rather than only looking at aggregate metrics.

**Why aggregate metrics hide bias:** A model can be 91% accurate overall while being 65% accurate for elderly customers — the large majority group's performance overwhelms the signal.

**How to do it:**
1. Define slices: (Age group) × (Gender) × (Nationality cluster)
2. Evaluate all core metrics per slice
3. Flag any slice where performance drops below a defined threshold (e.g., >10% degradation vs. overall)

**Tool:** `SliceFinder` (Google), or custom implementation using pandas groupby + scikit-learn metrics

---

### 3.5 Embedding Bias Testing (for LLM-based systems)

If you are using a Large Language Model (GPT-4o, Claude 3.5, Llama 3) as part of a customer-facing or decision-support system, bias can also live inside the model's text generation.

**Tests to run:**

- **Name bias test:** Ask the same customer service question but signed by "Mohammed Al-Rashid" vs. "John Smith" — does response quality or tone differ?
- **Sentiment consistency:** Does the model's summarization of a customer complaint change based on implied demographic signals in the complaint text?
- **Decision consistency:** For an LLM-assisted credit review system, present identical financial profiles with different implied demographics — does the recommended decision change?

**Tool:** Microsoft's `PyRIT` (Python Risk Identification Toolkit), custom prompt pairs

---

## 4. Implementation: Pre, During, and After Training

### Pre-Processing (Fix the data before training)

| Action | What to Do |
| :--- | :--- |
| **Dataset Profiling** | Analyze your training data distribution across protected attributes. Flag underrepresented groups. |
| **Resampling** | Oversample underrepresented groups or undersample overrepresented ones to balance the training set. |
| **Feature Audit** | Identify and flag proxy variables (postcode, device type, browser language) that correlate with protected attributes. |
| **Remove Leakage** | Ensure no protected attribute or its strong proxy leaks into the feature set. |

**Tool:** `aif360` resampling utilities, `pandas-profiling` / `ydata-profiling` for distribution analysis

---

### In-Processing (Constraints during model training)

| Action | What to Do |
| :--- | :--- |
| **Fairness Constraints** | Add fairness penalty terms to your loss function that penalize disparate impact during training. |
| **Adversarial Debiasing** | Train a secondary model that tries to predict the protected attribute from your primary model's outputs — optimize the primary model to fool it. |
| **Reweighting** | Assign higher sample weights to underrepresented groups during training. |

**Tool:** `aif360` in-processing algorithms (`AdversarialDebiasing`, `MetaFairClassifier`), `Fairlearn` constraints

---

### Post-Processing (Adjust outputs after the model is built)

| Action | What to Do |
| :--- | :--- |
| **Threshold Calibration** | Set different decision thresholds per demographic group to equalize True Positive Rates. |
| **Output Filtering** | Flag model outputs that would create disparate impact above defined thresholds for human review. |
| **Human-in-the-Loop Gates** | For high-stakes decisions (loan rejections above a certain amount), require a human reviewer when the model's confidence is below a defined threshold. |

**Tool:** `Fairlearn` threshold optimizer, custom human review queues

---

## 5. Fairness Metrics Reference

These are the metrics you will report to regulators. Know them cold.

| Metric | Formula | Pass Threshold |
| :--- | :--- | :--- |
| **Disparate Impact Ratio** | Min group approval rate / Max group approval rate | ≥ 0.80 |
| **Equal Opportunity Difference** | TPR(group A) − TPR(group B) | ≤ 0.05 |
| **Average Odds Difference** | Average of (FPR diff + TPR diff) / 2 | ≤ 0.05 |
| **Predictive Parity Difference** | Precision(group A) − Precision(group B) | ≤ 0.05 |

> These thresholds are not universal law — they are defensible defaults. Your compliance team may define different thresholds for specific use cases. Document whatever thresholds you choose and the rationale behind them.

---

## 6. The Regulatory Audit Checklist

When a regulator asks you to prove your model is not biased, this is your evidence package:

- [ ] **Bias Risk Assessment document** — written before deployment, identifying all potential bias vectors
- [ ] **Training data distribution report** — showing representation across all protected attribute groups
- [ ] **Disparate Impact Ratio report** — calculated on your validation and test sets
- [ ] **Counterfactual test results** — a sample of counterfactual pairs and the model's consistency rate
- [ ] **Slice-based performance report** — metrics broken down by demographic slice
- [ ] **Fairness constraint documentation** — what constraints were applied during training and why
- [ ] **Human review threshold documentation** — for which cases is a human required to review before the decision is executed
- [ ] **Ongoing monitoring logs** — showing that bias metrics are being tracked in production, not just at deployment time

---

## 7. Tools Summary

| Tool | Use Case | Language |
| :--- | :--- | :--- |
| **IBM AI Fairness 360 (aif360)** | End-to-end bias detection and mitigation | Python |
| **Fairlearn** | Fairness constraints, threshold optimization, dashboard | Python |
| **SHAP** | Explaining individual model decisions, identifying feature influence | Python |
| **Microsoft PyRIT** | LLM-specific bias and safety red-teaming | Python |
| **What-If Tool (Google)** | Visual counterfactual and slice analysis | Python / TF |
| **Responsible AI Toolbox (Microsoft)** | Integrated dashboard: bias + explainability + error analysis | Python |

---

> **Bottom Line:** You can't prove fairness without measuring it. Start measuring early, document everything, and treat your fairness metrics as first-class deployment gates — the same way you treat accuracy and latency.