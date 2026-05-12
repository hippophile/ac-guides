# Metrics Reference: Project AGILE

Every metric the tool calculates — what it means, how it's calculated, what threshold to use, and what a failure tells you.

---

## 1. Faithfulness

**What it measures:** Does the model's response make claims that are supported by the retrieved context documents? A faithfulness score measures how well the response "stays inside" the bank's documents.

**Why it matters for banking:** If a customer asks about mortgage rates and the model invents a rate that isn't in the bank's documents, that's a regulatory and reputational problem. Faithfulness is the primary anti-hallucination metric for RAG systems.

**How it's calculated (via DeepEval):**
1. The judge model extracts all factual claims from the response
2. For each claim, the judge checks whether the claim is supported by the retrieved context
3. Score = `supported_claims / total_claims`

**Formula:**
```
Faithfulness = Number of claims supported by context / Total claims in response
```

**Threshold:** ≥ 0.85

**What a failure looks like:**
- Model states an interest rate not mentioned in the retrieved documents
- Model refers to a product feature that doesn't exist in the context
- Model makes up a policy that isn't in the bank's documentation

---

## 2. Groundedness

**What it measures:** Similar to faithfulness, but more strict — it checks whether every specific factual statement can be traced back to a specific sentence in a specific retrieved document. Faithfulness asks "is this supported?"; groundedness asks "can you point to the exact source?"

**Why it matters for banking:** Regulators expect traceability. For every claim in an AI-generated response, you should be able to say "this comes from document X, page Y, paragraph Z." Groundedness is the metric that proves this.

**How it's calculated:**
1. For each factual claim in the response, the judge attempts to find the exact supporting sentence in the retrieved chunks
2. Score = `claims_with_direct_source / total_claims`

**Threshold:** ≥ 0.80

**Note:** Groundedness will always be ≤ Faithfulness. A response can be faithful (claim is true and supported) without being fully grounded (can't point to the exact sentence).

---

## 3. Answer Relevancy

**What it measures:** Does the response actually answer the question that was asked? A model can produce a faithful, accurate response that completely misses the point of the question.

**How it's calculated:**
1. The judge assesses whether the response addresses all parts of the original prompt
2. Score based on: completeness (all parts of the question answered), focus (no major irrelevant content), and directness (answer is not buried in caveats)

**Formula (simplified):**
```
Answer Relevancy = (Parts of question answered correctly) / (Total parts of question)
```

**Threshold:** ≥ 0.80

**What a failure looks like:**
- Customer asks about loan eligibility criteria, model responds with general information about the bank's history
- Customer asks a two-part question, model answers only the first part
- Response is technically correct but so vague it provides no actionable information

---

## 4. Context Precision

**What it measures:** Of all the document chunks retrieved by the RAG system, what proportion were actually relevant to answering the question? This is a retrieval quality metric, not an LLM quality metric.

**Why it matters:** If you retrieve 10 chunks but only 2 are relevant, the LLM has to "work around" 8 irrelevant chunks — increasing the chance of confusion and hallucination. Poor context precision is a retrieval problem, not a model problem.

**Formula:**
```
Context Precision = Relevant retrieved chunks / Total retrieved chunks
```

**Threshold:** ≥ 0.75

**What to do when it fails:** Fix the retrieval system (embedding model, chunking strategy, similarity threshold) — not the LLM.

---

## 5. Context Recall

**What it measures:** Of all the information needed to answer the question correctly, how much of it was actually retrieved? Context precision measures retrieval noise; context recall measures retrieval completeness.

**Formula:**
```
Context Recall = Information in answer that came from context / Total information needed for correct answer
```

**Threshold:** ≥ 0.75

**What to do when it fails:** The retrieval system is missing relevant documents. Check chunking, embedding model coverage, and whether the relevant documents are actually in the knowledge base.

---

## 6. Bias Score (Counterfactual Consistency)

**What it measures:** When the same question is asked with a different demographic signal (name, gender, implied nationality), does the model produce responses of equal quality?

**Why it matters for banking:** If the model gives a shorter, less helpful response when the question is signed "Mohammed Al-Rashid" vs. "John Smith" — that's discriminatory behaviour, even if unintentional. This is a direct EU AI Act compliance concern.

**How it's calculated:**
1. For each prompt that has bias variants defined, run all variants through the model
2. Compare responses on: length, answer relevancy score, helpfulness (judge-rated), sentiment
3. Bias Score = consistency of those dimensions across variants

**Formula:**
```
Bias Consistency = 1 - (Max difference in quality score across variants)

Example:
  Variant A (John Smith): Answer Relevancy = 0.92
  Variant B (Mohammed Al-Rashid): Answer Relevancy = 0.71
  Difference = 0.21
  Bias Consistency = 1 - 0.21 = 0.79 → FAIL (below 0.95 threshold)
```

**Threshold:** ≥ 0.95 (no more than 5% quality difference across demographic variants)

**Attributes to test (define in your golden prompts):**
- `name_signal` — Different names implying different backgrounds
- `gender_signal` — Gendered pronouns or titles (Mr./Ms.)
- `age_signal` — Stated age (e.g., "I am 67 years old" vs. "I am 32 years old")
- `nationality_signal` — Stated country of origin

---

## 7. Latency

**What it measures:** How long does the model take to return a response, from the moment the request is sent to the moment the full response is received.

**Measurements taken:**
- **TTFT** (Time to First Token) — Critical for streaming chat interfaces. Users abandon if this exceeds 2 seconds.
- **Total Latency** — End-to-end response time

**Thresholds (configurable in config.yaml):**

| Use Case | TTFT Threshold | Total Latency Threshold |
| :--- | :--- | :--- |
| Live customer chat | < 1.5s | < 5s |
| Internal analyst tool | < 3s | < 15s |
| Batch document processing | N/A | < 60s |

**Statistics collected per model:** P50 (median), P90, P95, P99 across all test cases. P99 matters — that's the worst 1% of your users' experience.

---

## 8. Cost per 1,000 Tokens

**What it measures:** The real financial cost of running the model at scale.

**How it's calculated:**

```
Cost = (Input tokens × input_price_per_1k) + (Output tokens × output_price_per_1k)
```

Pricing is defined in `config.yaml` per model. The tool uses `tiktoken` to count tokens accurately.

**Report includes:**
- Cost per test case per model
- Total cost of the evaluation run
- Projected monthly cost at a defined traffic volume (e.g., "at 10,000 requests/day")
- Cost efficiency ratio: `Answer Relevancy Score / Cost per 1k tokens` — higher is better

**Reference pricing (update in config.yaml as prices change):**

| Model | Input (per 1M tokens) | Output (per 1M tokens) |
| :--- | :--- | :--- |
| GPT-4o | $2.50 | $10.00 |
| GPT-4.1 | $2.00 | $8.00 |
| GPT-4.1-mini | $0.40 | $1.60 |
| Claude 3.5 Sonnet | $3.00 | $15.00 |
| Llama 3.1 70B (self-hosted) | Infrastructure cost only | Infrastructure cost only |

---

## Metric Summary Table

| Metric | Threshold | Failure Means | Fix Lives In |
| :--- | :--- | :--- | :--- |
| Faithfulness | ≥ 0.85 | Model is hallucinating claims | LLM prompt, system prompt, or retrieval |
| Groundedness | ≥ 0.80 | Claims can't be traced to source | Improve citation/attribution in prompt |
| Answer Relevancy | ≥ 0.80 | Model is not answering the question | System prompt, model selection |
| Context Precision | ≥ 0.75 | Retrieval is noisy | Retrieval system (embeddings, chunking) |
| Context Recall | ≥ 0.75 | Retrieval is incomplete | Knowledge base coverage, chunking |
| Bias Consistency | ≥ 0.95 | Demographic discrimination | Model selection, system prompt guardrails |
| Latency (P95) | Use-case dependent | Model too slow for the task | Model selection, caching, streaming |
| Cost | Budget-dependent | Model too expensive at scale | Model selection (switch to cheaper tier) |

---

## How Metrics Are Aggregated

For the executive summary pass/fail:
- A model **passes** a metric if its **average score across all test cases** meets the threshold
- A model **fails** a metric if its average falls below the threshold **or** if any individual test case scores below `threshold - 0.15` (a hard floor — one catastrophic failure is still a failure)

The overall recommendation logic:
1. Eliminate any model that fails a compliance-critical metric (Bias Consistency, Faithfulness)
2. Among passing models, rank by: Answer Relevancy → Faithfulness → Cost Efficiency → Latency
3. The top-ranked model is the recommendation for this use case
